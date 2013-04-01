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
        private IFileSystemData _fileSystemData;
        private FileSystemOptions _options;

        internal FileSystem(FileSystemOptions options)
        {
            _options = options;
            Root = new Folder();
        }

        public static bool Delete(FileSystem fileSystem)
        {
            return false;
        }

        public IFileSystemData FileSystemData
        {
            get { return _fileSystemData; }
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            return folder.Folders;
        }

        public IIndexNode Find(Folder folder, string name)
        {
            return folder.IndexNodes.FirstOrDefault(f => f.Name == name);
        }

        public void CreateFolder(Folder parentFolder, Folder folder)
        {
            parentFolder.IndexNodes.Add(folder);
            folder.Parent = parentFolder;
            // TODO: persist
        }

        public void Import(string source, Folder dest, string nameOfNewElement)
        {
            throw new NotImplementedException();
        }

        public void Export(IIndexNode source, string dest)
        {
            throw new NotImplementedException();
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

        public void Move(IIndexNode toMove, Folder dest, string nameOfMovedElement)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Folder folder, string nameOfTheElement)
        {
            throw new NotImplementedException();
        }

        public Folder Root { get; private set; }
    }
}
