using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Interfaces;

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

        internal FileSystem(FileSystemOptions options)
        {
            _options = options;
            Root = new RootFolder();
        }

        public void CreateFolder(Folder parentFolder, Folder folder)
        {
            CheckDisposed();

            parentFolder.IndexNodes.Add(folder);
            folder.Parent = parentFolder;
            // TODO: persist
        }

        public void Import(string source, Folder dest, string name)
        {
            CheckDisposed();

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

        public void Copy(IIndexNode toCopy, Folder dest, string nameOfCopiedElement)
        {
            CheckDisposed();

            throw new NotImplementedException();
        }

        public void Delete(IIndexNode node)
        {
            CheckDisposed();

            node.Parent.IndexNodes.Remove(node);
            node.Parent = null;
            // TODO: persist
        }

        public void Move(IIndexNode toMove, Folder dest, string newName)
        {
            CheckDisposed();

            toMove.Parent.IndexNodes.Remove(dest);
            toMove.Parent = dest;
            toMove.Name = newName;

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

            // TODO: free managed resources

        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("Resource was disposed.");
        }

    }
}
