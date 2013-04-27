using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using VFSBase.Exceptions;
using VFSBase.Interfaces;
using VFSBase.Persistence;

namespace VFSBase.Implementation
{
    internal sealed class FileSystem : IFileSystem
    {
        private readonly FileSystemOptions _options;
        private bool _disposed;
        private readonly BlockParser _blockParser;
        private readonly BlockAllocation _blockAllocation;
        private BlockManipulator _blockManipulator;
        private readonly Persistence _persistence;

        public FileSystemOptions FileSystemOptions
        {
            get { return _options; }
        }

        internal FileSystem(FileSystemOptions options)
        {
            _options = options;

            _blockManipulator = new BlockManipulator(_options);
            _blockParser = new BlockParser(_options);
            _persistence = new Persistence(_blockParser, _blockManipulator);
            _blockAllocation = _options.BlockAllocation;

            InitializeFileSystem();
        }

        private void InitializeFileSystem()
        {
            Root = ImportRootFolder();
        }

        private RootFolder ImportRootFolder()
        {
            var folder = _blockParser.ParseRootFolder(_blockManipulator.ReadBlock(0));

            if (folder != null) return folder;

            var root = new RootFolder();
            _blockManipulator.WriteBlock(0, _blockParser.NodeToBytes(root));

            return root;
        }

        public IEnumerable<IIndexNode> List(Folder folder)
        {
            CheckDisposed();
            return GetBlockList(folder).AsEnumerable();
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            return List(folder).OfType<Folder>();
        }

        public IEnumerable<VFSFile> Files(Folder folder)
        {
            return List(folder).OfType<VFSFile>();
        }

        public RootFolder Root { get; private set; }

        public Folder CreateFolder(Folder parentFolder, string name)
        {
            CheckDisposed();
            CheckName(name);

            if (Exists(parentFolder, name)) throw new AlreadyExistsException();

            var folder = new Folder(name) { Parent = parentFolder, BlockNumber = _blockAllocation.Allocate() };
            _persistence.Persist(folder);
            AppendBlockReference(parentFolder, folder.BlockNumber);

            return folder;
        }

        #region Import

        //TODO: test this method with recursive data
        public void Import(string source, Folder destination, string name, CallbacksBase importCallbacks)
        {
            CheckDisposed();
            CheckName(name);

            if (Directory.Exists(source)) CollectImportDirectoryTotals(source, importCallbacks);
            else if (File.Exists(source)) importCallbacks.TotalToProcess++;
            else throw new NotFoundException();

            if (Directory.Exists(source)) ImportDirectory(source, destination, name, importCallbacks);
            else if (File.Exists(source)) ImportFile(source, destination, name, importCallbacks);
            else throw new NotFoundException();

            importCallbacks.OperationCompleted(!importCallbacks.ShouldAbort());
        }

        private static void CollectImportDirectoryTotals(string source, CallbacksBase importCallbacks)
        {
            var info = new DirectoryInfo(source);
            importCallbacks.TotalToProcess++;

            foreach (var directoryInfo in info.GetDirectories())
                CollectImportDirectoryTotals(directoryInfo.FullName, importCallbacks);

            importCallbacks.TotalToProcess += info.GetFiles().Length;
        }

        private void ImportFile(string source, Folder destination, string name, CallbacksBase importCallbacks)
        {
            if (importCallbacks.ShouldAbort()) return;

            var f = new FileInfo(source);
            if (f.Length > _options.MaximumFileSize)
                throw new VFSException(
                    string.Format(
                        "File is too big. Maximum file size is {0}. You can adjust the BlockSize in the Options to allow bigger files.",
                        _options.MaximumFileSize));

            var file = CreateFile(source, destination, name);
            AppendBlockReference(destination, file.BlockNumber);

            importCallbacks.CurrentlyProcessed++;
        }

        // TODO: test this
        private void ImportDirectory(string source, Folder destination, string name, CallbacksBase importCallbacks)
        {
            if (importCallbacks.ShouldAbort()) return;

            var info = new DirectoryInfo(source);

            var newFolder = CreateFolder(destination, name);

            importCallbacks.CurrentlyProcessed++;

            foreach (var directoryInfo in info.GetDirectories())
                ImportDirectory(directoryInfo.FullName, newFolder, directoryInfo.Name, importCallbacks);

            foreach (var fileInfo in info.GetFiles())
                ImportFile(fileInfo.FullName, newFolder, fileInfo.Name, importCallbacks);
        }

        #endregion


        #region Export

        public void Export(IIndexNode source, string destination, CallbacksBase exportCallbacks)
        {
            var absoluteDestination = Path.GetFullPath(destination);
            EnsureParentDirectoryExists(absoluteDestination);
            CheckDisposed();

            if (source == null) throw new NotFoundException();

            if (File.Exists(absoluteDestination) || Directory.Exists(absoluteDestination))
                throw new VFSException("Destination already exists!");

            // Gather totals
            if (source is Folder) CollectExportDirectoryTotals(source as Folder, exportCallbacks);
            else if (source is VFSFile) exportCallbacks.TotalToProcess++;
            else throw new ArgumentException("Source must be of type Folder or VFSFile", "source");

            // Do the real export
            if (source is Folder) ExportFolder(source as Folder, absoluteDestination, exportCallbacks);
            else ExportFile(source as VFSFile, absoluteDestination, exportCallbacks);

            exportCallbacks.OperationCompleted(!exportCallbacks.ShouldAbort());
        }

        private void CollectExportDirectoryTotals(Folder source, CallbacksBase exportCallbacks)
        {
            // Direcotry
            exportCallbacks.TotalToProcess++;

            // File
            exportCallbacks.TotalToProcess += Files(source).Count();

            foreach (var folder in Folders(source))
            {
                CollectExportDirectoryTotals(folder, exportCallbacks);
            }
        }

        //TODO: test this
        private void ExportFolder(Folder folder, string destination, CallbacksBase exportCallbacks)
        {
            if (exportCallbacks.ShouldAbort()) return;

            Directory.CreateDirectory(destination);
            exportCallbacks.CurrentlyProcessed++;

            foreach (var vfsFile in Files(folder))
            {
                ExportFile(vfsFile, Path.Combine(destination, vfsFile.Name), exportCallbacks);
            }
            foreach (var f in Folders(folder))
            {
                ExportFolder(f, Path.Combine(destination, folder.Name), exportCallbacks);
            }
        }

        private void ExportFile(VFSFile file, string destination, CallbacksBase exportCallbacks)
        {
            if (exportCallbacks.ShouldAbort()) return;

            EnsureParentDirectoryExists(destination);
            using (var stream = File.OpenWrite(destination))
            {
                using (
                    var reader =
                        DecorateToHostStream(new VFSFileStream(file, _blockParser, _options, _blockAllocation, _blockManipulator,
                                                               _persistence)))
                {
                    var buffer = new byte[_options.BlockSize];
                    int read;
                    while ((read = reader.Read(buffer, 0, _options.BlockSize)) > 0)
                    {
                        stream.Write(buffer, 0, read);
                    }
                }
            }
            exportCallbacks.CurrentlyProcessed++;
        }

        private static void EnsureParentDirectoryExists(string destination)
        {
            var name = Path.GetFileName(destination);
            if (name == null || name.Length <= 0) throw new VFSException("Name invalid");
            var directoryName = Path.GetDirectoryName(destination);
            if (directoryName == null || !Directory.Exists(directoryName))
                throw new VFSException(string.Format("Directory {0} does not exist", directoryName));
        }

        #endregion


        #region Copy

        public void Copy(IIndexNode nodeToCopy, Folder destination, string name, CallbacksBase copyCallbacks)
        {
            CheckDisposed();
            CheckName(name);

            // Gather totals
            if (nodeToCopy is Folder) CollectExportDirectoryTotals(nodeToCopy as Folder, copyCallbacks);
            else if (nodeToCopy is VFSFile) copyCallbacks.TotalToProcess++;
            else throw new ArgumentException("nodeToCopy must be of type Folder or VFSFile", "nodeToCopy");

            // Do the real copy
            if (nodeToCopy is Folder) CopyFolder(nodeToCopy as Folder, destination, name, copyCallbacks);
            else CopyFile(nodeToCopy as VFSFile, destination, name, copyCallbacks);

            copyCallbacks.OperationCompleted(!copyCallbacks.ShouldAbort());
        }

        private void CopyFile(VFSFile fileToCopy, Folder destination, string name, CallbacksBase copyCallbacks)
        {
            if (copyCallbacks.ShouldAbort()) return;

            CheckName(name);

            var file = new VFSFile(name)
                           {
                               Parent = destination,
                               BlockNumber = _blockAllocation.Allocate(),
                               LastBlockSize = fileToCopy.LastBlockSize
                           };

            foreach (var block in GetBlockList(fileToCopy).Blocks()) AddDataToFile(file, block);

            _persistence.Persist(file);

            AppendBlockReference(destination, file.BlockNumber);

            copyCallbacks.CurrentlyProcessed++;
        }

        private void CopyFolder(Folder nodeToCopy, Folder destination, string name, CallbacksBase copyCallbacks)
        {
            if (copyCallbacks.ShouldAbort()) return;

            CheckName(name);

            var newFolder = CreateFolder(destination, name);
            copyCallbacks.CurrentlyProcessed++;

            foreach (var subNode in Folders(nodeToCopy)) CopyFolder(subNode, newFolder, subNode.Name, copyCallbacks);
            foreach (var subNode in Files(nodeToCopy)) CopyFile(subNode, newFolder, subNode.Name, copyCallbacks);
        }

        #endregion

        public void Delete(IIndexNode node)
        {
            CheckDisposed();

            GetBlockList(node.Parent).Remove(node);
        }

        public void Move(IIndexNode node, Folder destination, string name)
        {
            CheckDisposed();
            CheckName(name);

            if (Exists(destination, name)) throw new ArgumentException("Folder already exists!");

            var blockNumber = node.BlockNumber;
            GetBlockList(node.Parent).Remove(node, false);
            AppendBlockReference(destination, blockNumber);

            node.Name = name;

            _persistence.Persist(node);
        }

        public IIndexNode Find(Folder folder, string name)
        {
            CheckDisposed();

            return GetBlockList(folder).Find(name);
        }

        public bool Exists(Folder folder, string name)
        {
            CheckDisposed();

            return GetBlockList(folder).Exists(name);
        }

        private void AppendBlockReference(IIndexNode parentFolder, long reference)
        {
            GetBlockList(parentFolder).AddReference(reference);
        }

        private IBlockList GetBlockList(IIndexNode parentFolder)
        {
            return new BlockList(parentFolder, _blockAllocation, _options, _blockParser, _blockManipulator, _persistence);
        }

        private VFSFile CreateFile(string source, Folder destination, string name)
        {
            var file = new VFSFile(name) { Parent = destination, BlockNumber = _blockAllocation.Allocate() };

            using (var b = new BinaryReader(File.OpenRead(source)))
            using (var w = DecorateToVFSStream(new VFSFileStream(file, _blockParser, _options, _blockAllocation, _blockManipulator, _persistence)))
            {
                byte[] block;
                while ((block = b.ReadBytes(_options.BlockSize)).Length > 0)
                {
                    w.Write(block, 0, block.Length);
                }
            }

            //Note: we could save some metadata too...

            return file;
        }

        private Stream DecorateToVFSStream(Stream originalStream)
        {
            return _options.StreamCodingStrategy.DecorateToVFS(originalStream);
        }

        private Stream DecorateToHostStream(Stream originalStream)
        {
            return _options.StreamCodingStrategy.DecorateToHost(originalStream);
        }

        private void AddDataToFile(VFSFile file, byte[] block)
        {
            var nextBlockNumber = _blockAllocation.Allocate();
            _blockManipulator.WriteBlock(nextBlockNumber, block);
            AppendBlockReference(file, nextBlockNumber);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource.

            if (!disposing) return;

            _disposed = true;

            // free managed resources

            if (_blockManipulator != null)
            {
                WriteConfig();
                _blockManipulator.Dispose();
                _blockManipulator = null;
            }
        }

        private void WriteConfig()
        {
            _blockManipulator.SaveConfig(_options, _blockAllocation);
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("Resource was disposed.");
        }

        private void CheckName(string name)
        {
            if (name != PathParser.NormalizeName(name)) throw new VFSException("Name invalid");
            if (name.Length <= 0) throw new VFSException("Name must not be empty!");
            if (BlockParser.StringToBytes(name).Length > _options.NameLength)
                throw new VFSException(string.Format("Name too long, max {0}", 255));
        }
    }
}
