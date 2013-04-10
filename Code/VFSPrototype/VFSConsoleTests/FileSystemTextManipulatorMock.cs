using System.Collections.Generic;
using VFSBase.Interfaces;

namespace VFSConsoleTests
{
    internal class FileSystemTextManipulatorMock : IFileSystemTextManipulator
    {
        public bool FolderExists = false;
        public IList<string> CurrentFolders;
        public bool IsCurrentDirectory = false;

        public IList<string> Folders(string path)
        {
            return CurrentFolders;
        }

        public bool IsDirectory(string path)
        {
            return IsCurrentDirectory;
        }

        public void CreateFolder(string path)
        {
        }

        public void Import(string source, string dest)
        {
        }

        public void Export(string source, string dest)
        {
        }

        public void Copy(string source, string dest)
        {
        }

        public void Delete(string path)
        {
        }

        public void Move(string source, string dest)
        {
        }

        public bool Exists(string path)
        {
            return FolderExists;
        }

        public void Dispose()
        {
        }
    }
}
