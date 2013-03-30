using System.Collections.Generic;
using VFSBase;

namespace VFSConsoleTests
{
    internal class FileSystemManipulatorMock : IFileSystemManipulator
    {
        public bool FolderExists = false;
        public IEnumerable<string> CurrentFolders;
        public bool IsCurrentDirectory = false;

        public IEnumerable<string> Folders(string path)
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

        public void ImportFile(string source, string dest)
        {
        }

        public void ExportFile(string source, string dest)
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

    }
}