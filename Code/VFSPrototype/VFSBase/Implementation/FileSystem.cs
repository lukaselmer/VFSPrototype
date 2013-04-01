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

        internal FileSystem(FileSystemOptions options)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool IsDirectory(Folder folder)
        {
            throw new NotImplementedException();
        }

        public void CreateFolder(Folder folder)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Move(IIndexNode toMove, Folder dest, string nameOfMovedElement)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Folder folder, string nameOfTheElement)
        {
            throw new NotImplementedException();
        }
    }
}
