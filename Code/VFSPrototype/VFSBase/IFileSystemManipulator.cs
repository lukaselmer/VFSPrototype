﻿using System.Collections.Generic;

namespace VFSBase
{
    public interface IFileSystemManipulator
    {
        ISet<Folder> Folders { get; }
        Folder Folder(string path);
        void CreateFolder(string path);
        bool DoesFolderExist(string path);
        void DeleteFolder(string path);
        void ImportFile(string source, string dest);
        bool FileExists(string path);
    }
}