using System;
using System.Collections.Generic;
using VFSBase;

namespace VFSConsoleTests
{
    internal class FileSystemManipulatorMock : IFileSystemManipulator
    {
        public FileSystemManipulatorMock()
        {
            
        }

        public bool FolderExists = false;
        public IEnumerable<string> _folders;

        public IEnumerable<string> Folders(string path)
        {
            return _folders;
        }

        public void CreateFolder(string path)
        {
        }

        public bool DoesFolderExist(string path)
        {
            return FolderExists;
        }

        public void DeleteFolder(string path)
        {
        }

        public void ImportFile(string source, string dest)
        {
        }

        public void ExportFile(string source, string dest)
        {
            throw new NotImplementedException();
        }

        public void Copy(string source, string dest)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public void Move(string source, string dest)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string path)
        {
            throw new NotImplementedException();
        }

        public bool DoesFileExists(string path)
        {
            throw new NotImplementedException();
        }
    }
}