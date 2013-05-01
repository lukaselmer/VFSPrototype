﻿using System;
using System.Collections.Generic;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    public interface IFileSystemTextManipulator : IDisposable
    {
        IList<string> Search(string keyword, string folder, bool recursive, bool caseSensitive);
        IList<string> Files(string path);
        IList<string> List(string path);
        IList<string> Folders(string path);
        bool IsDirectory(string path);
        void CreateFolder(string path);
        void Import(string source, string dest, CallbacksBase importCallbacks = null);
        void Export(string source, string dest, CallbacksBase exportCallbacks = null);
        void Copy(string source, string dest, CallbacksBase copyCallbacks = null);
        void Delete(string path);
        void Move(string source, string dest);
        bool Exists(string path);

        IFileSystemOptions FileSystemOptions { get; }
    }
}
