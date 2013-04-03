using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Interfaces;
using VFSBase.Persistance;

namespace VFSBase.Implementation
{

    /*using (var reader = new BinaryReader(stream))
    {
        DiskSize = reader.ReadUInt64();
        MasterBlockSize = reader.ReadUInt32();
    }*/

    //var writer = new BinaryWriter(stream);
    //writer.Write(DiskSize);
    //writer.Write(MasterBlockSize);

    internal class FileSystem : IFileSystem
    {
        private readonly FileSystemOptions _options;
        private bool _disposed;
        private FileStream _disk;
        private BinaryReader _diskReader;
        private BinaryWriter _diskWriter;
        private readonly BlockParser _blockParser;

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
        }

        private void SeekToBlock(long blockNumber)
        {
            _disk.Seek(_options.MasterBlockSize + (blockNumber * _options.BlockSize), SeekOrigin.Begin);
        }


        private Folder ImportRootFolder()
        {
            // TODO: import root folder
            SeekToBlock(0);
            var b = _diskReader.ReadBytes(_options.BlockSize);
            _blockParser.ParseBlock(b);

            return new RootFolder();
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            CheckDisposed();

            return folder.IndexNodes.OfType<Folder>();
        }

        public IIndexNode Find(Folder folder, string name)
        {
            CheckDisposed();

            return folder.IndexNodes.FirstOrDefault(f => f.Name == name);
        }

        public Folder Root { get; private set; }


        public void CreateFolder(Folder parentFolder, Folder folder)
        {
            CheckDisposed();
            CheckName(folder.Name);

            if (Exists(parentFolder, folder.Name)) throw new ArgumentException("Folder already exists!");

            parentFolder.IndexNodes.Add(folder);
            folder.Parent = parentFolder;
            // TODO: persist
        }

        public void Import(string source, Folder dest, string name)
        {
            CheckDisposed();
            CheckName(name);

            var file = new VFSFile(name, source);
            dest.IndexNodes.Add(file);
            file.Parent = dest;
            // TODO: persist
        }

        public void Export(IIndexNode source, string dest)
        {
            CheckDisposed();

            var file = source as VFSFile;
            if (file == null) throw new FileNotFoundException();
            File.WriteAllBytes(dest, file.Data);
        }

        public void Copy(IIndexNode toCopy, Folder dest, string name)
        {
            CheckDisposed();
            CheckName(name);

            throw new NotImplementedException();
        }

        public void Delete(IIndexNode node)
        {
            CheckDisposed();

            node.Parent.IndexNodes.Remove(node);
            node.Parent = null;
            // TODO: persist
        }

        public void Move(IIndexNode toMove, Folder dest, string name)
        {
            CheckDisposed();
            CheckName(name);

            if (Exists(dest, name)) throw new ArgumentException("Folder already exists!");

            toMove.Parent.IndexNodes.Remove(dest);
            toMove.Parent = dest;
            toMove.Name = name;

            toMove.Parent = dest;
            dest.IndexNodes.Add(toMove);

            // TODO: persist
        }

        public bool Exists(Folder folder, string name)
        {
            CheckDisposed();

            return folder.IndexNodes.Any(i => i.Name == name);
        }

        public FileSystemOptions FileSystemOptions
        {
            get { return _options; }
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
