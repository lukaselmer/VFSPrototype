using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Interfaces;
using VFSBase.Persistance;
using VFSBase.Persistance.Blocks;

namespace VFSBase.Implementation
{
    internal class FileSystem : IFileSystem
    {
        private readonly FileSystemOptions _options;
        private bool _disposed;
        private FileStream _disk;
        private BinaryReader _diskReader;
        private BinaryWriter _diskWriter;
        private readonly BlockParser _blockParser;
        private long _nextFreeBlock;

        public FileSystemOptions FileSystemOptions { get { return _options; } }

        internal FileSystem(FileSystemOptions options)
        {
            _options = options;
            _blockParser = new BlockParser(_options);

            _disk = new FileStream(_options.Location, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, _options.BlockSize, FileOptions.RandomAccess);
            _diskReader = new BinaryReader(_disk);
            _diskWriter = new BinaryWriter(_disk);

            InitializeFileSystem();
        }

        private void InitializeFileSystem()
        {
            Root = ImportRootFolder();
            _nextFreeBlock = 1;
        }

        private void SeekToBlock(long blockNumber)
        {
            _disk.Seek(_options.MasterBlockSize + (blockNumber * _options.BlockSize), SeekOrigin.Begin);
        }

        private RootFolder ImportRootFolder()
        {
            var folder = _blockParser.ParseRootFolder(ReadBlock(0));

            if (folder != null) return folder;

            var root = new RootFolder();
            WriteBlock(0, _blockParser.NodeToBytes(root));

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
            var b = _blockParser.BytesToNode(ReadBlock(blockNumber));
            b.BlockNumber = blockNumber;
            return b;
        }

        public IIndexNode Find(Folder folder, string name)
        {
            CheckDisposed();

            return Folders(folder).FirstOrDefault(f => f.Name == name);
        }

        public RootFolder Root { get; private set; }

        /* NOTE: this method appends the reference to the *unordered* list. It would be faster if
         * the nodes would be sorted by name, so searching could be achieved in O(log(n)).
         * 
         * Possible improvement: implement an AVL tree!
         */
        public void CreateFolder(Folder parentFolder, Folder folder)
        {
            CheckDisposed();
            CheckName(folder.Name);

            if (Exists(parentFolder, folder.Name)) throw new ArgumentException("Folder already exists!");

            folder.Parent = parentFolder;

            var blockToStoreNewFolder = _nextFreeBlock++;

            var newFolderBytes = _blockParser.NodeToBytes(folder);
            WriteBlock(blockToStoreNewFolder, newFolderBytes);

            AppendBlockReference(parentFolder, blockToStoreNewFolder);
        }

        public void Import(string source, Folder dest, string name)
        {
            //TODO: implement this, with persistence

            CheckDisposed();
            CheckName(name);

            //var file = new VFSFile(name, source);
            //dest.IndexNodes.Add(file);
            //file.Parent = dest;
            // TODO: persist
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

            if (node.Parent.BlocksCount == 0) return;

            node.Parent.BlocksCount--;
            if (node.Parent.BlocksCount == 0)
            {
                node.Parent.IndirectNodeNumber = 0;
                Persist(node.Parent);
                return;
            }

            WriteBlock(node.Parent.BlockNumber, _blockParser.NodeToBytes(node.Parent));

            var blocksCount = node.Parent.BlocksCount;
            var refsCount = _options.ReferencesPerIndirectNode;

            var indexIndirection2 = (int)(blocksCount / (refsCount * refsCount));
            var indexIndirection1 = (int)((blocksCount - (indexIndirection2 * refsCount * refsCount)) / refsCount);
            var indexIndirection0 = (int)(blocksCount - (indexIndirection2 * refsCount * refsCount) - (refsCount * indexIndirection1));

            var indirectNode3 = ReadIndirectNode(node.Parent.IndirectNodeNumber);
            var indirectNode2 = ReadIndirectNode(indirectNode3.BlockNumbers[indexIndirection2]);
            var indirectNode1 = ReadIndirectNode(indirectNode2.BlockNumbers[indexIndirection1]);

            var refToMove = indirectNode1.BlockNumbers[indexIndirection0];

            // The file contents could be destroyed, but it is not necessary.
            // Disabled, so "Move" can be implemented simpler.
            // WriteBlock(node.BlockNumber, new byte[_options.BlockSize]);

            indirectNode1.BlockNumbers[indexIndirection0] = 0;
            Persist(indirectNode1);

            if (refToMove == node.BlockNumber) return;

            ReplaceInIndirectNode(indirectNode3, node.BlockNumber, refToMove, 2);
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
            Persist(toMove);
        }

        public bool Exists(Folder folder, string name)
        {
            CheckDisposed();
            return Folders(folder).Any(i => i.Name == name);
        }

        private void ReplaceInIndirectNode(IndirectNode indirectNode, long toBeReplaced, long toReplace, int recursion)
        {
            for (var i = 0; i < indirectNode.BlockNumbers.Length; i++)
            {
                var blockNumber = indirectNode.BlockNumbers[i];
                if (blockNumber == 0) return;

                if (recursion == 0)
                {
                    if (blockNumber == toBeReplaced)
                    {
                        indirectNode.BlockNumbers[i] = toReplace;
                        Persist(indirectNode);
                        return;
                    }
                }
                else ReplaceInIndirectNode(ReadIndirectNode(blockNumber), toBeReplaced, toReplace, recursion - 1);
            }
        }

        private void AppendBlockReference(Folder parentFolder, long reference)
        {
            var indirectNodeNumber = parentFolder.IndirectNodeNumber;
            if (indirectNodeNumber == 0)
            {
                parentFolder.IndirectNodeNumber = CreateIndirectNode().BlockNumber;
            }

            var blocksCount = parentFolder.BlocksCount;
            var refsCount = _options.ReferencesPerIndirectNode;

            var indexIndirection2 = (int)(blocksCount / (refsCount * refsCount));
            var indexIndirection1 = (int)((blocksCount - (indexIndirection2 * refsCount * refsCount)) / refsCount);
            var indexIndirection0 = (int)(blocksCount - (indexIndirection2 * refsCount * refsCount) - (refsCount * indexIndirection1));

            parentFolder.BlocksCount += 1;
            Persist(parentFolder);

            var indirectNode3 = ReadIndirectNode(parentFolder.IndirectNodeNumber);
            if (indirectNode3.BlockNumbers[indexIndirection2] == 0)
            {
                indirectNode3.BlockNumbers[indexIndirection2] = CreateIndirectNode().BlockNumber;
                Persist(indirectNode3);
            }

            var indirectNode2 = ReadIndirectNode(indirectNode3.BlockNumbers[indexIndirection2]);
            if (indirectNode2.BlockNumbers[indexIndirection1] == 0)
            {
                indirectNode2.BlockNumbers[indexIndirection1] = CreateIndirectNode().BlockNumber;
                Persist(indirectNode2);
            }

            var indirectNode1 = ReadIndirectNode(indirectNode2.BlockNumbers[indexIndirection1]);
            indirectNode1.BlockNumbers[indexIndirection0] = reference;
            Persist(indirectNode1);
        }

        private IndirectNode CreateIndirectNode()
        {
            var newNodeNumber = _nextFreeBlock++;
            var indirectNode = new IndirectNode(new long[_options.ReferencesPerIndirectNode]) { BlockNumber = newNodeNumber };
            Persist(indirectNode);
            return indirectNode;
        }

        private IndirectNode ReadIndirectNode(long indirectNodeNumber)
        {
            var node = _blockParser.ParseIndirectNode(ReadBlock(indirectNodeNumber));
            node.BlockNumber = indirectNodeNumber;
            return node;
        }

        private void WriteBlock(long blockNumber, byte[] block)
        {
            SeekToBlock(blockNumber);
            _diskWriter.Write(block);
        }

        private byte[] ReadBlock(long blockNumber)
        {
            SeekToBlock(blockNumber);
            var block = _diskReader.ReadBytes(_options.BlockSize);
            if (block.Length != _options.BlockSize) return new byte[_options.BlockSize];
            return block;
        }

        private void Persist(IIndexNode node)
        {
            //NOTE: this could be better!
            if (node is Folder) Persist(node as Folder);
            else if (node is VFSFile) Persist(node as VFSFile);
        }

        private void Persist(Folder folder)
        {
            WriteBlock(folder.BlockNumber, _blockParser.NodeToBytes(folder));
        }

        private void Persist(VFSFile file)
        {
            WriteBlock(file.BlockNumber, _blockParser.NodeToBytes(file));
        }

        private void Persist(IndirectNode indirectNode)
        {
            WriteBlock(indirectNode.BlockNumber, _blockParser.NodeToBytes(indirectNode));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource.

            if (!disposing) return;

            _disposed = true;

            // free managed resources

            if (_disk != null)
            {
                _disk.Flush(true);
                _disk.Dispose();
                _disk = null;
            }

            if (_diskReader != null)
            {
                _diskReader.Dispose();
                _diskReader = null;
            }

            if (_diskWriter != null)
            {
                _diskWriter.Dispose();
                _diskWriter = null;
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
