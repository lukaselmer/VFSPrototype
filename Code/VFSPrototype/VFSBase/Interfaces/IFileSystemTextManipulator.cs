using System;
using System.Collections.Generic;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    public interface IFileSystemTextManipulator : IDisposable
    {
        IList<string> Files(string path);
        IList<string> List(string path);
        IList<string> Folders(string path);
        bool IsDirectory(string path);
        void CreateFolder(string path);
        void Import(string source, string dest, ImportCallbacks importCallbacks = null);
        void Export(string source, string dest, ExportCallbacks exportCallbacks = null);
        void Copy(string source, string dest, CopyCallbacks copyCallbacks = null);
        void Delete(string path);
        void Move(string source, string dest);
        bool Exists(string path);

        IFileSystemOptions FileSystemOptions { get; }
    }
}
