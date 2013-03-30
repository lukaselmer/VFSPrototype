﻿using System;
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
        public Folder CurrentFolder;

        public ISet<Folder> Folders { get; private set; }

        public Folder Folder(string path)
        {
            return CurrentFolder;
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
            throw new NotImplementedException();
        }

        public void ImportFile(string source, string dest)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }
    }
}