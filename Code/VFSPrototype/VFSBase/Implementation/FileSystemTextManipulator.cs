using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Exceptions;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    public class FileSystemTextManipulator : IFileSystemTextManipulator
    {
        private IFileSystem _fileSystem;

        public FileSystemTextManipulator(FileSystemOptions options)
        {
            _fileSystem = FileSystemFactory.CreateOrImport(options);
        }

        public IList<string> Folders(string path)
        {
            var node = FindNode(path);

            var folder = node as Folder;
            if (folder == null) throw new DirectoryNotFoundException();

            return _fileSystem.Folders(folder).Select(f => f.Name).ToList();
        }

        public IList<string> Files(string path)
        {
            var node = FindNode(path);

            var folder = node as Folder;
            if (folder == null) throw new DirectoryNotFoundException();

            return _fileSystem.Files(folder).Select(f => f.Name).ToList();
        }

        public IList<string> List(string path)
        {
            var node = FindNode(path);

            var folder = node as Folder;
            if (folder == null) throw new DirectoryNotFoundException();

            return _fileSystem.List(folder).Select(f => f.Name).ToList();
        }

        public bool IsDirectory(string path)
        {
            return FindNode(path) as Folder != null;
        }

        public void CreateFolder(string path)
        {
            if (IsDirectory(path)) return;

            var parentFolderPath = PathParser.GetParent(path);
            CreateFolder(parentFolderPath);

            var parentFolder = FindParentFolder(path);

            if (parentFolder == null) throw new DirectoryNotFoundException();

            _fileSystem.CreateFolder(parentFolder, PathParser.GetNodeName(path));
        }

        public void Delete(string path)
        {
            var node = FindNode(path);

            if (node == null) throw new NotFoundException();

            _fileSystem.Delete(node);
        }

        public void Move(string source, string dest)
        {
            var nodeToMove = FindNode(source);
            if (nodeToMove == null) throw new NotFoundException();

            if (Exists(dest)) throw new VFSException("Element already exists");

            var destParentFolderPath = PathParser.GetParent(dest);
            CreateFolder(destParentFolderPath);
            var parent = FindNode(destParentFolderPath) as Folder;

            _fileSystem.Move(nodeToMove, parent, PathParser.GetNodeName(dest));
        }

        public bool Exists(string path)
        {
            var folders = PathToQueue(path);

            // the root folder always exists
            if (folders.Count == 0) return true;

            var parent = FindParentFolder(path);
            if (parent == null) return false;

            return _fileSystem.Exists(parent, PathParser.GetNodeName(path));
        }

        public void Import(string source, string dest)
        {
            var node = CreateParentFolder(dest);
            _fileSystem.Import(source, node, PathParser.GetNodeName(dest));
        }

        private Folder CreateParentFolder(string dest)
        {
            CreateFolder(PathParser.GetParent(dest));
            return FindParentFolder(dest);
        }

        public void Export(string source, string dest)
        {
            _fileSystem.Export(FindNode(source), dest);
        }

        public void Copy(string source, string dest)
        {
            if (!Exists(source)) throw new VFSException(string.Format("Source {0} does not exist", source));
            if (Exists(dest)) throw new VFSException(string.Format("Destination {0} already exists", dest));

            CreateParentFolder(dest);
            _fileSystem.Copy(FindNode(source), FindParentFolder(dest), PathParser.GetNodeName(dest));
        }

        private static Queue<string> PathToQueue(string path)
        {
            return new Queue<string>(PathParser.SplitPath(path));
        }

        private IIndexNode FindNode(string path)
        {
            return FindNode(PathToQueue(path), _fileSystem.Root);
        }

        private IIndexNode FindParentNode(string path)
        {
            return FindNode(PathToQueue(PathParser.GetParent(path)), _fileSystem.Root);
        }

        private IIndexNode FindNode(Queue<string> folders, IIndexNode node)
        {
            if (!folders.Any()) return node;

            var folder = node as Folder;
            if (folder == null) return null;

            var folderName = folders.Dequeue();
            var subFolder = _fileSystem.Find(folder, folderName);

            return subFolder == null ? null : FindNode(folders, subFolder);
        }

        private Folder FindParentFolder(string path)
        {
            return FindParentNode(path) as Folder;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource.

            if (!disposing) return;

            // free managed resources
            if (_fileSystem == null) return;

            _fileSystem.Dispose();
            _fileSystem = null;
        }
    }
}
