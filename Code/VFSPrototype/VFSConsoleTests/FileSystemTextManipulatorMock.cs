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

        public void Import(string source, string dest, Func<bool> shouldAbort = null, Action<bool> operationCompleted = null, Action<int> totalToProcessChanged = null, Action<int> currentlyProcessedChanged = null)
        {
        }

        public void Export(string source, string dest, Func<bool> shouldAbort = null, Action<bool> operationCompleted = null, Action<int> totalToProcessChanged = null, Action<int> currentlyProcessedChanged = null)
        {
        }

        public void Copy(string source, string dest, Func<bool> shouldAbort = null, Action<bool> operationCompleted = null)
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

        public void Dispose()
        {
        }
    }
}
