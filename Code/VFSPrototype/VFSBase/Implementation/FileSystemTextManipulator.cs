using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    public class FileSystemTextManipulator : IFileSystemTextManipulator
    {
        private readonly IFileSystem _fileSystem;

        public FileSystemTextManipulator(FileSystemOptions options)
        {
            _fileSystem = FileSystemFactory.CreateOrImport(options);
        }

        public IList<string> Folders(string path)
        {
            var node = FindNode(_fileSystem.Root, ParsePath(path));

            var folder = node as Folder;
            if (folder == null) throw new DirectoryNotFoundException();

            return _fileSystem.Folders(folder).Select(f => f.Name).ToList();
        }

        private IIndexNode FindNode(IIndexNode node, Queue<string> folders)
        {
            if (!folders.Any()) return node;

            var folder = node as Folder;
            if(folder == null) return null;

            var folderName = folders.Dequeue();
            var subFolder = _fileSystem.Find(folder, folderName);

            return subFolder == null ? null : FindNode(subFolder, folders);
        }

        public bool IsDirectory(string path)
        {
            return FindNode(_fileSystem.Root, ParsePath(path)) as Folder != null;
        }

        public void CreateFolder(string path)
        {
            if (IsDirectory(path)) return;

            var parentFolderPath = PathParser.GetParent(path);
            CreateFolder(parentFolderPath);

            var parentFolder = FindNode(_fileSystem.Root, ParsePath(PathParser.GetParent(path))) as Folder;

            if (parentFolder == null) throw new DirectoryNotFoundException();

            _fileSystem.CreateFolder(parentFolder, new Folder(PathParser.GetFilename(path)));
        }

        public void Delete(string path)
        {
            var node = FindNode(_fileSystem.Root, ParsePath(path));

            if (node == null) throw new NotFoundException();

            _fileSystem.Delete(node);
        }

        public void Move(string source, string dest)
        {
            var sourceFolders = ParsePath(source);
            var node = _fileSystem.Root.Delete(sourceFolders);

            var last = dest.LastIndexOf('/');
            var folder = last >= 0 ? dest.Substring(0, last) : "";
            var name = last >= 0 ? dest.Substring(last + 1) : dest;
            var destFolders = ParsePath(folder);

            node.Name = name;
            _fileSystem.Root.Insert(destFolders, node);
        }

        private static Queue<string> ParsePath(string path)
        {
            return new Queue<string>(PathParser.SplitPath(path));
        }

        public bool Exists(string path)
        {
            var folders = ParsePath(path);

            var isRoot = folders.Count == 0;

            return isRoot || _fileSystem.Root.Exists(folders);
        }


        public void ImportFile(string source, string dest)
        {
            var path = new Queue<string>(PathParser.NormalizePath(dest).Split(PathParser.PathSeperator));
            _fileSystem.Root.ImportFile(path, source);
        }

        public void ExportFile(string source, string dest)
        {
            var path = new Queue<string>(PathParser.NormalizePath(source).Split(PathParser.PathSeperator));
            _fileSystem.Root.ExportFile(path, dest);
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
