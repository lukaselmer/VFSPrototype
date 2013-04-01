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
            if (folder == null) return null;

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

            var parentFolder = FindParentFolder(path);

            if (parentFolder == null) throw new DirectoryNotFoundException();

            _fileSystem.CreateFolder(parentFolder, new Folder(PathParser.GetNodeName(path)));
        }

        public void Delete(string path)
        {
            var node = FindNode(_fileSystem.Root, ParsePath(path));

            if (node == null) throw new NotFoundException();

            _fileSystem.Delete(node);
        }

        public void Move(string source, string dest)
        {
            var nodeToMove = FindNode(_fileSystem.Root, ParsePath(source));
            if (nodeToMove == null) throw new NotFoundException();

            if (Exists(dest)) throw new VFSException("Element already exists");

            var destParentFolderPath = PathParser.GetParent(dest);
            CreateFolder(destParentFolderPath);
            var parent = FindNode(_fileSystem.Root, ParsePath(destParentFolderPath)) as Folder;

            _fileSystem.Move(nodeToMove, parent, PathParser.GetNodeName(dest));
        }

        public bool Exists(string path)
        {
            var folders = ParsePath(path);

            // the root folder always exists
            if (folders.Count == 0) return true;

            var parent = FindParentFolder(path);
            if(parent == null) return false;

            return _fileSystem.Exists(parent, PathParser.GetNodeName(path));
        }


        public void ImportFile(string source, string dest)
        {
            _fileSystem.Root.ImportFile(ParsePath(dest), source);
        }

        public void ExportFile(string source, string dest)
        {
            _fileSystem.Root.ExportFile(ParsePath(source), dest);
        }

        public void Copy(string source, string dest)
        {
            throw new System.NotImplementedException();
        }

        private static Queue<string> ParsePath(string path)
        {
            return new Queue<string>(PathParser.SplitPath(path));
        }

        private Folder FindParentFolder(string path)
        {
            var folders = ParsePath(path);

            if (folders.Count == 0) return _fileSystem.Root;

            return FindNode(_fileSystem.Root, ParsePath(PathParser.GetParent(path))) as Folder;
        }

    }
}
