﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using VFSBase.Callbacks;
using VFSBase.Exceptions;
using VFSBase.Helpers;
using VFSBase.Interfaces;
using VFSBase.Persistence;
using VFSBase.Search;
using VFSBlockAbstraction;

namespace VFSBase.Implementation
{
    internal class FileSystem : IFileSystem
    {
        #region Fields and properties

        private FileSystemOptions _options;
        private bool _disposed;
        private BlockParser _blockParser;
        private BlockAllocation _blockAllocation;
        private BlockManipulator _blockManipulator;
        private Persistence.Persistence _persistence;
        public Folder Root { get; private set; }
        private Folder LatestRoot { get; set; }

        /// <summary>
        /// The lock is used to lock the file system, so the file system can be used in multiple threads.
        /// Especially useful for the synchronization.
        /// </summary>
        private ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public FileSystemOptions FileSystemOptions
        {
            get { return _options; }
        }

        #endregion


        #region Constructor / Initialization

        internal FileSystem(FileSystemOptions options)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                _options = options;

                _blockManipulator = new BlockManipulator(_options.Location, _options.BlockSize, _options.MasterBlockSize);
                _blockParser = new BlockParser(_options);
                _persistence = new Persistence.Persistence(_blockParser, _blockManipulator);
                _blockAllocation = _options.BlockAllocation;

                InitializeFileSystem();
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public void Reload(FileSystemOptions options)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                _options = options;

                _blockAllocation = _options.BlockAllocation;
                // TODO: reload indexing service
                // SearchService.StartIndexing(_searchService, this);

                Root = LatestRoot = ImportRootFolder();
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public void OnFileSystemChanged(object sender, FileSystemChangedEventArgs e)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                Root = LatestRoot = ImportRootFolder();
                if (FileSystemChanged != null) FileSystemChanged(sender, e);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public event EventHandler<FileSystemChangedEventArgs> FileSystemChanged;

        private void InitializeFileSystem()
        {
            Root = LatestRoot = ImportRootFolder();

        }

        private Folder ImportRootFolder()
        {
            var folder = _blockParser.ParseFolder(_blockManipulator.ReadBlock(_options.RootBlockNr));
            if (folder != null)
            {
                folder.BlockNumber = _options.RootBlockNr;
                return folder;
            }

            var root = new Folder { IsRoot = true };
            _blockManipulator.WriteBlock(root.BlockNumber, _blockParser.NodeToBytes(root));

            return root;
        }

        public void TestEncryptionKey()
        {
            var check = "0123456789".ToArray().Select(c => (byte)c).ToArray();
            byte[] ret;
            using (var ms = new MemoryStream())
            {
                using (var encryptor = _options.StreamCodingStrategy.DecorateToVFS(ms))
                {
                    encryptor.Write(check, 0, check.Length);
                    encryptor.Flush();
                }
                ret = ms.ToArray();
            }

            if (_options.EncryptionStringForTest == null)
            {
                _options.EncryptionStringForTest = ret;
            }

            if (_options.EncryptionStringForTest.Where((t, i) => t != ret[i]).Any())
            {
                throw new VFSException("invalid password");
            }
        }

        #endregion


        #region Versioning

        public long LatestVersion
        {
            get { return LatestRoot.Version; }
        }

        public long CurrentVersion
        {
            get { return Root.Version; }
        }

        public void SwitchToLatestVersion()
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                Root = LatestRoot;
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public void SwitchToVersion(long version)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                if (version < 0) throw new VFSException(string.Format("Version {0} is not a positive version", version));
                if (LatestRoot.Version < version) throw new VFSException(string.Format("Version {0} does not exist", version));
                if (Root.Version < version) Root = LatestRoot;

                while (version < Root.Version)
                {
                    var blockNr = Root.PredecessorBlockNr;
                    var readBlock = _blockManipulator.ReadBlock(blockNr);
                    Root = _blockParser.ParseFolder(readBlock);
                    Root.BlockNumber = blockNr;
                }
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        private long NextVersion
        {
            get { return Root.Version + 1; }
        }

        private void CheckVersion()
        {
            if (Root.Version != LatestRoot.Version) throw new VFSException(string.Format(
                 "Changes can only be made on the latest version. This version is {0}, latest version is {1}", Root.Version, LatestRoot.Version));
        }

        private Folder ArchiveAndReplaceRoot(Folder folderToCopy, IIndexNode nodeToReplace, IIndexNode nodeReplacement)
        {
            if (folderToCopy == null) throw new VFSException("Node cannot be null");

            var toCopy = folderToCopy;
            var toReplace = nodeToReplace;
            var replacement = nodeReplacement;

            Folder copyOfFolderToCopy = null;
            Folder previous = null;

            while (toCopy != null)
            {
                var newFolder = GetBlockList(toCopy).CopyReplacingReference(toCopy, toReplace, replacement, NextVersion);
                if (copyOfFolderToCopy == null) copyOfFolderToCopy = newFolder;

                if (previous != null)
                {
                    previous.Parent = newFolder;
                    _persistence.Persist(previous);
                }

                toReplace = toCopy;
                toCopy = toCopy.Parent;
                replacement = newFolder;

                previous = newFolder;
            }

            Debug.Assert(previous != null, "previous != null");
            Debug.Assert(string.IsNullOrEmpty(previous.Name), "previous.Name == \"\"");

            // previous is now the new root node!
            previous.IsRoot = true;
            _persistence.Persist(previous);
            ResetRoot(previous);

            return copyOfFolderToCopy;
        }

        private void ResetRoot(Folder newRoot)
        {
            newRoot.Version = NextVersion;

            Root = newRoot;
            Root.IsRoot = true;
            LatestRoot = Root;
            Root.BlocksUsed = _blockAllocation.CurrentMax;
            _persistence.Persist(Root);
            _options.RootBlockNr = Root.BlockNumber;
            WriteConfig();
        }


        #endregion


        #region Create

        public Folder CreateFolder(Folder parentFolder, string name)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                return CreateFolder(parentFolder, name, true);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        private Folder CreateFolder(Folder parentFolder, string name, bool createVersion)
        {
            CheckDisposed();
            CheckName(name);
            CheckVersion();

            if (Exists(parentFolder, name)) throw new AlreadyExistsException();

            var newParentFolder = createVersion ? ArchiveAndReplaceRoot(parentFolder, null, null) : parentFolder;

            var folder = new Folder(name)
                             {
                                 Parent = newParentFolder,
                                 BlockNumber = _blockAllocation.Allocate(),
                                 Version = newParentFolder.Version
                             };
            _persistence.Persist(folder);

            AppendBlockReference(newParentFolder, folder.BlockNumber);

            return folder;
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

            //Note: we could save some metadata too..
            return file;
        }

        #endregion


        #region Import

        public void Import(string source, Folder destination, string name, CallbacksBase importCallbacks)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                CheckDisposed();
                CheckName(name);
                CheckVersion();

                destination = ArchiveAndReplaceRoot(destination, null, null);

                if (Directory.Exists(source)) CollectImportDirectoryTotals(source, importCallbacks);
                else if (File.Exists(source)) importCallbacks.TotalToProcess++;
                else throw new NotFoundException();

                if (Directory.Exists(source)) ImportDirectory(source, destination, name, importCallbacks, false);
                else if (File.Exists(source)) ImportFile(source, destination, name, importCallbacks);
                else throw new NotFoundException();

                importCallbacks.OperationCompleted(!importCallbacks.ShouldAbort());
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
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
                    string.Format("File is too big. Maximum file size is {0}. You can adjust the BlockSize in the Options to allow bigger files.",
                    _options.MaximumFileSize));

            var file = CreateFile(source, destination, name);
            AppendBlockReference(destination, file.BlockNumber);

            importCallbacks.CurrentlyProcessed++;
        }

        private void ImportDirectory(string source, Folder destination, string name, CallbacksBase importCallbacks, bool createVersion)
        {
            if (importCallbacks.ShouldAbort()) return;

            var info = new DirectoryInfo(source);

            var newFolder = CreateFolder(destination, name, createVersion);

            importCallbacks.CurrentlyProcessed++;

            foreach (var directoryInfo in info.GetDirectories())
                ImportDirectory(directoryInfo.FullName, newFolder, directoryInfo.Name, importCallbacks, false);

            foreach (var fileInfo in info.GetFiles())
                ImportFile(fileInfo.FullName, newFolder, fileInfo.Name, importCallbacks);
        }

        #endregion


        #region Export

        public void Export(IIndexNode source, string destination, CallbacksBase exportCallbacks)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                var absoluteDestination = Path.GetFullPath(destination);
                EnsureParentDirectoryExists(absoluteDestination);
                CheckDisposed();

                if (source == null) throw new NotFoundException();

                if (File.Exists(absoluteDestination) || Directory.Exists(absoluteDestination)) throw new VFSException("Destination already exists!");

                // Gather totals
                if (source is Folder) CollectExportDirectoryTotals(source as Folder, exportCallbacks);
                else if (source is VFSFile) exportCallbacks.TotalToProcess++;
                else throw new ArgumentException("Source must be of type Folder or VFSFile", "source");

                // Do the real export
                if (source is Folder) ExportFolder(source as Folder, absoluteDestination, exportCallbacks);
                else ExportFile(source as VFSFile, absoluteDestination, exportCallbacks);

                exportCallbacks.OperationCompleted(!exportCallbacks.ShouldAbort());
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
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
                ExportFolder(f, Path.Combine(destination, f.Name), exportCallbacks);
            }
        }

        private void ExportFile(VFSFile file, string destination, CallbacksBase exportCallbacks)
        {
            if (exportCallbacks.ShouldAbort()) return;

            EnsureParentDirectoryExists(destination);
            using (var stream = File.OpenWrite(destination))
            {
                using (var reader = DecorateToHostStream(
                    new VFSFileStream(file, _blockParser, _options, _blockAllocation, _blockManipulator, _persistence)))
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
            _readWriteLock.EnterWriteLock();
            try
            {
                CheckDisposed();
                CheckName(name);
                CheckVersion();

                // Gather totals (copy in ~O(1) :D)
                copyCallbacks.TotalToProcess++;

                // Do the real copy
                if (nodeToCopy is Folder) CopyFolder(nodeToCopy as Folder, destination, name, copyCallbacks);
                else if (nodeToCopy is VFSFile) CopyFile(nodeToCopy as VFSFile, destination, name, copyCallbacks);
                else throw new ArgumentException("nodeToCopy must be of type Folder or VFSFile", "nodeToCopy");

                copyCallbacks.OperationCompleted(!copyCallbacks.ShouldAbort());
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        private void CopyFile(VFSFile fileToCopy, Folder destination, string name, CallbacksBase copyCallbacks)
        {
            if (copyCallbacks.ShouldAbort()) return;

            CheckName(name);

            var newFile = new VFSFile(name)
                           {
                               Parent = destination,
                               BlockNumber = _blockAllocation.Allocate(),
                               LastBlockSize = fileToCopy.LastBlockSize,
                               IndirectNodeNumber = fileToCopy.IndirectNodeNumber,
                               BlocksCount = fileToCopy.BlocksCount,
                               PredecessorBlockNr = fileToCopy.BlockNumber,
                               Version = NextVersion
                           };

            _persistence.Persist(newFile);
            copyCallbacks.CurrentlyProcessed++;

            ArchiveAndReplaceRoot(destination, null, newFile);

            /*foreach (var block in GetBlockList(fileToCopy).Blocks()) AddDataToFile(file, block);

            _persistence.Persist(file);

            AppendBlockReference(destination, file.BlockNumber);

            copyCallbacks.CurrentlyProcessed++;

            _searchService.AddToIndex(file);*/
        }

        private void CopyFolder(Folder folderToCopy, Folder destination, string name, CallbacksBase copyCallbacks)
        {
            if (copyCallbacks.ShouldAbort()) return;

            CheckName(name);

            var copiedFolder = CreateFolder(destination, name, true);
            copiedFolder.IndirectNodeNumber = folderToCopy.IndirectNodeNumber;
            copiedFolder.BlocksCount = folderToCopy.BlocksCount;
            copiedFolder.PredecessorBlockNr = folderToCopy.BlockNumber;
            //copiedFolder.Version = NextVersion;
            _persistence.Persist(copiedFolder);

            //ArchiveAndReplaceRoot(copiedFolder.Parent, null, copiedFolder);
            copyCallbacks.CurrentlyProcessed++;
        }

        #endregion


        #region Delete

        public void Delete(IIndexNode node)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                CheckDisposed();
                CheckVersion();

                if (node.Parent == null) throw new VFSException("Cannot delete root node");
                ArchiveAndReplaceRoot(node.Parent, node, null);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        #endregion


        #region Search / Find / Exists

        public IEnumerable<IIndexNode> List(Folder folder)
        {
            _readWriteLock.EnterReadLock();
            try
            {
                CheckDisposed();
                return GetBlockList(folder).AsEnumerable().ToList();
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            _readWriteLock.EnterReadLock();
            try
            {
                return List(folder).OfType<Folder>().ToList();
            }
            finally
            {

                _readWriteLock.ExitReadLock();
            }
        }

        public IEnumerable<VFSFile> Files(Folder folder)
        {
            _readWriteLock.EnterReadLock();
            try
            {
                return List(folder).OfType<VFSFile>().ToList();
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
        }

        public IIndexNode Find(Folder folder, string name)
        {
            _readWriteLock.EnterReadLock();
            try
            {
                CheckDisposed();

                return GetBlockList(folder).Find(name);
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
        }

        public bool Exists(Folder folder, string name)
        {
            _readWriteLock.EnterReadLock();
            try
            {
                CheckDisposed();

                return GetBlockList(folder).Exists(name);
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
        }

        #endregion


        #region Blocks and Streams

        private void AppendBlockReference(IIndexNode parentFolder, long reference)
        {
            GetBlockList(parentFolder).AddReference(reference);
        }

        private IBlockList GetBlockList(IIndexNode parentFolder)
        {
            return new BlockList(parentFolder, _blockAllocation, _options, _blockParser, _blockManipulator, _persistence);
        }

        private Stream DecorateToVFSStream(Stream originalStream)
        {
            return _options.StreamCodingStrategy.DecorateToVFS(originalStream);
        }

        private Stream DecorateToHostStream(Stream originalStream)
        {
            return _options.StreamCodingStrategy.DecorateToHost(originalStream);
        }

        /*private void AddDataToFile(VFSFile file, byte[] block)
        {
            var nextBlockNumber = _blockAllocation.Allocate();
            _blockManipulator.WriteBlock(nextBlockNumber, block);
            AppendBlockReference(file, nextBlockNumber);
        }*/

        #endregion


        #region Dispose / Cleanup

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

            if (_readWriteLock != null)
            {
                _readWriteLock.Dispose();
                _readWriteLock = null;
            }
        }

        public void WriteConfig()
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                CheckVersion();
                _blockManipulator.SaveConfig(_options);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        #endregion


        #region Checks / Preconditions

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

        #endregion


        #region Synchronization

        public ReaderWriterLockSlim GetReadWriteLock()
        {
            return _readWriteLock;
        }

        public bool IsSynchronizedDisk { get { return _options.Id != 0; } }

        public void MakeSynchronizedDisk(int id)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                if (id == 0) throw new ArgumentException("id cannot be null or empty", "id");
                if (IsSynchronizedDisk) throw new VFSException("Disk is synchronized already");
                _options.Id = id;
                _options.LocalVersion = 0;
                _options.LastServerVersion = 0;
                WriteConfig();
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public byte[] ReadBlock(long blockNumber)
        {
            _readWriteLock.EnterReadLock();
            try
            {
                return _blockManipulator.ReadBlock(blockNumber);
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
        }

        public void WriteBlock(long blockNumber, byte[] block)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                _blockManipulator.WriteBlock(blockNumber, block);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public void WriteFileSystemOptions(byte[] serializedFileSystemOptions)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                _blockManipulator.SaveConfig(serializedFileSystemOptions);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        #endregion

    }
}
