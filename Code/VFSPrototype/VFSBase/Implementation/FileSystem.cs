using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Interfaces;
using VFSBase.Persistence;
using VFSBase.Persistence.Blocks;

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

        public FileSystemOptions FileSystemOptions { get { return _options; } }

        internal FileSystem(FileSystemOptions options)
        {
            _options = options;

            _blockManipulator = new BlockManipulator(_options);
            _blockParser = new BlockParser(_options);
            _persistence = new Persistence(_blockParser, _blockManipulator);
            _blockAllocation = new BlockAllocation(_options);

            InitializeFileSystem();

            //TODO: set the pointer to the next free block when imporing the file system

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

        public IEnumerable<Folder> Folders(Folder folder)
        {
            CheckDisposed();

            var l = new List<IIndexNode>((int)folder.BlocksCount);
            if (folder.IndirectNodeNumber == 0) return l.OfType<Folder>();

            AddFromIndirectNode(ReadIndirectNode(folder.IndirectNodeNumber), l, 2);

            l.ForEach(f => f.Parent = folder);

            return l.OfType<Folder>();
        }

        private void AddFromIndirectNode(IndirectNode indirectNode, List<IIndexNode> l, int recursion)
        {
            foreach (var blockNumber in indirectNode.BlockNumbers)
            {
                if (blockNumber == 0) return;

                if (recursion == 0) l.Add(ReadIndexNode(blockNumber));
                else AddFromIndirectNode(ReadIndirectNode(blockNumber), l, recursion - 1);
            }
        }

        private IIndexNode ReadIndexNode(long blockNumber)
        {
            var b = _blockParser.BytesToNode(_blockManipulator.ReadBlock(blockNumber));
            b.BlockNumber = blockNumber;
            return b;
        }

        public IIndexNode Find(Folder folder, string name)
        {
            CheckDisposed();

            return Folders(folder).FirstOrDefault(f => f.Name == name);
        }

        public RootFolder Root { get; private set; }

        public void CreateFolder(Folder parentFolder, string name)
        {
            CheckDisposed();
            CreateFolderInternal(parentFolder, name);
        }

        /* NOTE: this method appends the reference to the *unordered* list. It would be faster if
         * the nodes would be sorted by name, so searching could be achieved in O(log(n)).
         * 
         * Possible improvement: implement an AVL tree!
         */
        private Folder CreateFolderInternal(Folder parentFolder, string name)
        {
            CheckName(name);

            if (Exists(parentFolder, name)) throw new ArgumentException("Folder already exists!");

            var folder = new Folder(name) { Parent = parentFolder, BlockNumber = _blockAllocation.Allocate() };

            _persistence.PersistFolder(folder);

            AppendBlockReference(parentFolder, folder.BlockNumber);

            return folder;
        }

        public void Import(string source, Folder dest, string name)
        {
            //TODO: test this method, recursivly

            CheckDisposed();
            CheckName(name);

            if (Directory.Exists(source))
            {

                // TODO: test this
                var info = new DirectoryInfo(source);

                var newFolder = CreateFolderInternal(dest, name);

                // TODO: test this
                foreach (var directoryInfo in info.GetDirectories())
                    Import(directoryInfo.FullName, newFolder, directoryInfo.Name);

                // TODO: test this
                foreach (var fileInfo in info.GetFiles())
                    Import(fileInfo.FullName, newFolder, fileInfo.Name);

            }
            else if (File.Exists(source))
            {
                // Note: this should be tested already
                var file = CreateFile(source, dest, name);
                AppendBlockReference(dest, file.BlockNumber);
            }
            else
            {
                throw new NotFoundException();
            }
        }

        public void Export(IIndexNode source, string dest)
        {
            //TODO: implement this, with persistence

            CheckDisposed();

            var file = source as VFSFile;
            if (file == null) throw new FileNotFoundException();
            File.WriteAllBytes(dest, file.Data);
        }

        public void Copy(IIndexNode toCopy, Folder dest, string name)
        {
            //TODO: implement this, with persistence

            CheckDisposed();
            CheckName(name);

            throw new NotImplementedException();
        }

        public void Delete(IIndexNode node)
        {
            CheckDisposed();

            var l = new BlockList(node, _blockAllocation, _options, _blockParser, _blockManipulator, _persistence);
            l.Delete(node.IndirectNodeNumber);
        }

        public void Move(IIndexNode toMove, Folder dest, string name)
        {
            CheckDisposed();
            CheckName(name);

            if (Exists(dest, name)) throw new ArgumentException("Folder already exists!");

            var blockNumber = toMove.BlockNumber;
            Delete(toMove);
            AppendBlockReference(dest, blockNumber);

            toMove.Name = name;
            _persistence.Persist(toMove);
        }

        public bool Exists(Folder folder, string name)
        {
            CheckDisposed();
            return Folders(folder).Any(i => i.Name == name);
        }


        private void AppendBlockReference(Folder parentFolder, long reference)
        {
            var l = new BlockList(parentFolder, _blockAllocation, _options, _blockParser, _blockManipulator, _persistence);
            l.Add(reference);
        }

        private VFSFile CreateFile(string source, Folder dest, string name)
        {
            // TODO: implement this
            //var file = new VFSFile(name, source);
            //dest.IndexNodes.Add(file);
            //file.Parent = dest;
            // TODO: persist
            throw new NotImplementedException();
        }

        private IndirectNode ReadIndirectNode(long indirectNodeNumber)
        {
            return ReadIndirectNodeStatic(_blockManipulator, _blockParser, indirectNodeNumber);
        }

        public static IndirectNode ReadIndirectNodeStatic(BlockManipulator blockManipulator, BlockParser blockParser, long indirectNodeNumber)
        {
            var readBlock = blockManipulator.ReadBlock(indirectNodeNumber);
            var node = blockParser.ParseIndirectNode(readBlock);
            node.BlockNumber = indirectNodeNumber;
            return node;
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
                _blockManipulator.Dispose();
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("Resource was disposed.");
        }

        private void CheckName(string name)
        {
            if (name != PathParser.NormalizeName(name)) throw new VFSException("Name invalid");
            if (name.Length <= 0) throw new VFSException("Name must not be empty!");
            if (BlockParser.StringToBytes(name).Length > _options.NameLength) throw new VFSException(string.Format("Name too long, max {0}", 255));
        }

    }
}
