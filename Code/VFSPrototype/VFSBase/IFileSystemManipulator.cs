﻿using System.Collections.Generic;

namespace VFSBase
{
    public interface IFileSystemManipulator
    {
        IEnumerable<string> Folders(string path);
        void CreateFolder(string path);
        void ImportFile(string source, string dest);
        void ExportFile(string source, string dest);
        void Copy(string source, string dest);
        void Delete(string path);
        void Move(string source, string dest);
        bool Exists(string path);
    }
}