using System;
using System.Collections.Generic;

namespace VFSBase.Interfaces
{
    public interface IFileSystemTextManipulator : IDisposable
    {
        IList<string> Files(string path);
        IList<string> List(string path);
        IList<string> Folders(string path);
        bool IsDirectory(string path);
        void CreateFolder(string path);
        void Import(string source, string dest, Func<bool> shouldAbort = null, Action<bool> operationCompleted = null, Action<int> totalToProcessChanged = null, Action<int> currentlyProcessedChanged = null);
        void Export(string source, string dest, Func<bool> shouldAbort = null, Action<bool> operationCompleted = null, Action<int> totalToProcessChanged = null, Action<int> currentlyProcessedChanged = null);
        void Copy(string source, string dest, Func<bool> shouldAbort = null, Action<bool> operationCompleted = null);
        void Delete(string path);
        void Move(string source, string dest);
        bool Exists(string path);
    }
}
