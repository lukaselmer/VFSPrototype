using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    class FileSystem : IFileSystem
    {
        private readonly FileSystemOptions _options;

        internal FileSystem(FileSystemOptions options)
        {
            _options = options;
            Root = new Folder();
        }

        /*public static bool Delete(FileSystem fileSystem)
        {
            return false;
        }*/

        public void CreateFolder(Folder parentFolder, Folder folder)
        {
           // if (_disposed)
           //     throw new ObjectDisposedException("Resource was disposed.");

            parentFolder.IndexNodes.Add(folder);
            folder.Parent = parentFolder;
            // TODO: persist
        }

        public void Import(string source, Folder dest, string name)
        {
            var file = new VFSFile(name, source);
            dest.IndexNodes.Add(file);
            file.Parent = dest;
            // TODO: persist
        }

        public void Export(IIndexNode source, string dest)
        {
            var file = source as VFSFile;
            if (file == null) throw new FileNotFoundException();
            File.WriteAllBytes(dest, file.Data);
        }

        public void Copy(IIndexNode toCopy, Folder dest, string nameOfCopiedElement)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexNode node)
        {
            node.Parent.IndexNodes.Remove(node);
            node.Parent = null;
            // TODO: persist
        }

        public void Move(IIndexNode toMove, Folder dest, string newName)
        {
            toMove.Parent.IndexNodes.Remove(dest);
            toMove.Parent = dest;
            toMove.Name = newName;

            toMove.Parent = dest;
            dest.IndexNodes.Add(toMove);

            // TODO: persist
        }

        public bool Exists(Folder folder, string name)
        {
            return folder.IndexNodes.Any(i => i.Name == name);
        }

        public FileSystemOptions FileSystemOptions
        {
            get { return _options; }
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            return folder.IndexNodes.OfType<Folder>();
        }

        public IIndexNode Find(Folder folder, string name)
        {
            return folder.IndexNodes.FirstOrDefault(f => f.Name == name);
        }

        public Folder Root { get; private set; }
        public void Dispose()
        {
            //Dispose(true);
           // GC.SuppressFinalize(this);    
        }
        /*protected void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource. 
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_resource != null)
                        _resource.Dispose();
                    Console.WriteLine("Object disposed.");
                }

                // Indicate that the instance has been disposed.
                _resource = null;
                _disposed = true;
            }
        }*/
    }
}
