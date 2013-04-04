using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly BlockManipulator _blockManipulator;
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

        public IEnumerable<IIndexNode> AsEnumerable(Folder folder)
        {
            CheckDisposed();
            return GetBlockList(folder).AsEnumerable();
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            CheckDisposed();
            return AsEnumerable(folder).OfType<Folder>();
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

        //TODO: test this method with recursive data
        public void Import(string source, Folder destination, string name)
        {
            CheckDisposed();
            CheckName(name);

            if (Directory.Exists(source)) ImportDirectory(source, destination, name);
            else if (File.Exists(source)) ImportFile(source, destination, name);
            else throw new NotFoundException();
        }

        public void Export(IIndexNode source, string destination)
        {
            CheckDisposed();

            if (source == null) throw new NotFoundException();

            if (File.Exists(destination) || Directory.Exists(destination)) throw new VFSException("Destination already exists!");

            if (source is Folder) ExportFolder(source as Folder, destination);
            else if (source is VFSFile) ExportFile(source as VFSFile, destination);
            else throw new ArgumentException("Source must be of type Folder or VFSFile", "source");
        }

        private void ExportFile(VFSFile vfsFile, string destination)
        {
            using (var w = new BinaryWriter(File.OpenWrite(destination)))
            {
                GetBlockList(vfsFile).WriteToStream(w);
            }
        }

        private void ExportFolder(Folder folder, string destination)
        {
            // TODO: implement this
            throw new NotImplementedException();
        }

        public void Copy(IIndexNode node, Folder destination, string name)
        {
            //TODO: implement this, with persistence

            CheckDisposed();
            CheckName(name);

            throw new NotImplementedException();
        }

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

        private void ImportFile(string source, Folder destination, string name)
        {
            var file = CreateFile(source, destination, name);
            AppendBlockReference(destination, file.BlockNumber);
        }

        // TODO: test this
        private void ImportDirectory(string source, Folder destination, string name)
        {
            var info = new DirectoryInfo(source);

            var newFolder = CreateFolder(destination, name);

            foreach (var directoryInfo in info.GetDirectories())
                ImportDirectory(directoryInfo.FullName, newFolder, directoryInfo.Name);

            foreach (var fileInfo in info.GetFiles())
                ImportFile(fileInfo.FullName, newFolder, fileInfo.Name);
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
            {
                byte[] block;
                while ((block = b.ReadBytes(_options.BlockSize)).Length == _options.BlockSize)
                {
                    AddDataToFile(file, block);
                }
                var lastBlockSize = block.Length;

                if (block.Length > 0)
                {
                    var lastBlock = new byte[_options.BlockSize];
                    block.CopyTo(lastBlock, 0);
                    AddDataToFile(file, block);
                }

                file.LastBlockSize = lastBlockSize;
                _persistence.Persist(file);
            }

            //Note: we could save some metadata too...

            return file;
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
