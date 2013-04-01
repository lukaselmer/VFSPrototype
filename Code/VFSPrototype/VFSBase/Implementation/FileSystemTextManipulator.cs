using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    public class FileSystemTextManipulator : IFileSystemTextManipulator
    {
        private readonly FileSystemData _fileSystemData;

        public FileSystemTextManipulator(FileSystemData fileSystemData)
        {
            _fileSystemData = fileSystemData;
        }

        public IEnumerable<string> Folders (string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            return _fileSystemData.Root.GetFolder(folders).Folders.Select(folder => folder.Name);
        }

        public bool IsDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateFolder(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            _fileSystemData.Root.CreateFolder(folders);
        }

        public void Delete(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            _fileSystemData.Root.Delete(folders);
        }

        public void Move (string source, string dest)
        {
            var sourceFolders = new Queue<string>(source.Split('/'));
            IIndexNode node = _fileSystemData.Root.Delete(sourceFolders);

            var last = dest.LastIndexOf('/');
            var folder = last >= 0 ? dest.Substring(0, last) : "";
            var name = last >= 0 ? dest.Substring(last+1) : dest;
            var destFolders = new Queue<string>(folder.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries));

            node.Name = name;
            _fileSystemData.Root.Insert(destFolders, node);

        }

        
        public bool Exists(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            return _fileSystemData.Root.Exists(folders);
        }


        public void ImportFile(string source, string dest)
        {
            var path = new Queue<string>(dest.Split('/'));   
            _fileSystemData.Root.ImportFile(path, source);
        }

        public void ExportFile(string source, string dest)
        {
            var path = new Queue<string>(source.Split('/'));
            _fileSystemData.Root.ExportFile(path, dest);
        }

        public void Copy(string source, string dest)
        {
            throw new System.NotImplementedException();
        }

        public void MoveFile(string source, string dest)
        {
            throw new System.NotImplementedException();
        }
    }
}
