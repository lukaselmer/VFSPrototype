using System;
using System.Collections.Generic;
using System.Linq;
using VFSBase.Exceptions;
using VFSBase.Interfaces;

namespace VFSConsoleTests
{
    internal class FileSystemTextManipulatorMock : IFileSystemTextManipulator
    {
        public bool FolderExists;
        public IList<string> CurrentFiles;
        public IList<string> CurrentFolders;
        public bool IsCurrentDirectory;
        public Exception ThrowException;

        public IList<string> Search(string keyword, string folder, bool recursive, bool caseSensitive)
        {
            throw new NotImplementedException();
        }

        public IList<string> Files(string path)
        {
            return CurrentFiles;
        }

        public IList<string> List(string path)
        {
            return CurrentFolders.Concat(CurrentFiles).ToList();
        }

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

        public void Import(string source, string dest, CallbacksBase importCallbacks = null)
        {
        }

        public void Export(string source, string dest, CallbacksBase exportCallbacks)
        {
        }

        public void Copy(string source, string dest, CallbacksBase copyCallbacks = null)
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
            if (ThrowException != null) throw ThrowException;
            return FolderExists;
        }

        public IFileSystemOptions FileSystemOptions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
        }
    }
}
