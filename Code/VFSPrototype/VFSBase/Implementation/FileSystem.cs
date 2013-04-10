﻿using System;
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
            var absoluteDestination = Path.GetFullPath(destination);
            EnsureParentDirectoryExists(absoluteDestination);
            CheckDisposed();

            if (source == null) throw new NotFoundException();

            if (File.Exists(absoluteDestination) || Directory.Exists(absoluteDestination)) throw new VFSException("Destination already exists!");

            if (source is Folder) ExportFolder(source as Folder, absoluteDestination);
            else if (source is VFSFile) ExportFile(source as VFSFile, absoluteDestination);
            else throw new ArgumentException("Source must be of type Folder or VFSFile", "source");
        }

        private void ExportFile(VFSFile file, string destination)
        {
            EnsureParentDirectoryExists(destination);
            using (var stream = File.OpenWrite(destination))
            using (var w = new BinaryWriter(stream))
            {
                GetBlockList(file).WriteToStream(w);
                if (file.LastBlockSize > 0)
                {
                    w.Seek(_options.BlockSize - file.LastBlockSize, SeekOrigin.End);
                    stream.SetLength(stream.Length + file.LastBlockSize - _options.BlockSize);
                }
            }
        }

        private static void EnsureParentDirectoryExists(string destination)
        {
            var name = Path.GetFileName(destination);
            if (name == null || name.Length <= 0) throw new VFSException("Name invalid");
            var directoryName = Path.GetDirectoryName(destination);
            if (directoryName == null || !Directory.Exists(directoryName))
                throw new VFSException(string.Format("Directory {0} does not exist", directoryName));
        }

        private void ExportFolder(Folder folder, string destination)
        {
            // TODO: implement this
            throw new NotImplementedException();
        }

        public void Copy(IIndexNode nodeToCopy, Folder destination, string name)
        {
            CheckDisposed();
            CheckName(name);

            if (nodeToCopy is Folder) CopyFolder(nodeToCopy as Folder, destination, name);
            else if (nodeToCopy is VFSFile) CopyFile(nodeToCopy as VFSFile, destination, name);
            else throw new ArgumentException("nodeToCopy must be of type Folder or VFSFile", "nodeToCopy");
        }

        private void CopyFile(VFSFile fileToCopy, Folder destination, string name)
        {
            CheckName(name);

            var file = new VFSFile(name) { Parent = destination, BlockNumber = _blockAllocation.Allocate(), LastBlockSize = fileToCopy.LastBlockSize };

            foreach (var block in GetBlockList(fileToCopy).Blocks()) AddDataToFile(file, block);

            _persistence.Persist(file);

            AppendBlockReference(destination, file.BlockNumber);
        }

        private void CopyFolder(Folder nodeToCopy, Folder destination, string name)
        {
            CheckName(name);

            var newFolder = CreateFolder(destination, name);
            foreach (var subNode in List(nodeToCopy)) Copy(subNode, newFolder, subNode.Name);
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
            var f = new FileInfo(source);
            if (f.Length > _options.MaximumFileSize) throw new VFSException(
                 string.Format("File is too big. Maximum file size is {0}. You can adjust the BlockSize in the Options to allow bigger files.", _options.MaximumFileSize));

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

                file.LastBlockSize = block.Length;
                if (file.LastBlockSize > 0)
                {
                    var lastBlock = new byte[_options.BlockSize];
                    block.CopyTo(lastBlock, 0);
                    AddDataToFile(file, block);
                }

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
