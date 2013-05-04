﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VFSBase.Exceptions;
using VFSBase.Interfaces;
using VFSBase.Search;

namespace VFSBase.Implementation
{
    public sealed class FileSystemTextManipulator : IFileSystemTextManipulator
    {
        private IFileSystem _fileSystem;

        public IFileSystemOptions FileSystemOptions { get { return _fileSystem.FileSystemOptions; } }

        public FileSystemTextManipulator(string location, string password)
        {
            _fileSystem = FileSystemFactory.Import(location, password);
            _fileSystem.TestEncryptionKey();
        }

        public FileSystemTextManipulator(FileSystemOptions options, string password)
        {
            _fileSystem = FileSystemFactory.Create(options, password);
            _fileSystem.TestEncryptionKey();
        }

        public IList<string> Search(string keyword, string folder, bool recursive, bool caseSensitive)
        {
            var restrictToFolder = FindNode(folder) as Folder;
            var searchOptions = new SearchOptions()
                {
                    Keyword = keyword,
                    CaseSensitive = caseSensitive,
                    RecursionDistance = (recursive ? -1 : 0),
                    RestrictToFolder = restrictToFolder ?? _fileSystem.Root
                };

            return _fileSystem.Search(searchOptions).Select(GetPath).ToList();
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

        public void Import(string source, string dest, CallbacksBase importCallbacks = null)
        {
            if (importCallbacks == null) importCallbacks = new ImportCallbacks();
            var node = CreateParentFolder(dest);
            _fileSystem.Import(source, node, PathParser.GetNodeName(dest), importCallbacks);
        }

        private Folder CreateParentFolder(string dest)
        {
            CreateFolder(PathParser.GetParent(dest));
            return FindParentFolder(dest);
        }

        public void Export(string source, string dest, CallbacksBase exportCallbacks)
        {
            if (exportCallbacks == null) exportCallbacks = new ExportCallbacks();
            _fileSystem.Export(FindNode(source), dest, exportCallbacks);
        }

        public void Export(string testFileSource, string dest, CallbacksBase exportCallbacks, long version)
        {
            var originalVersion = Version("/");
            try
            {
                SwitchToVersion(version);
                Export(testFileSource, dest, exportCallbacks);
            }
            finally
            {
                SwitchToVersion(originalVersion);
            }
        }

        public void Copy(string source, string dest, CallbacksBase copyCallbacks = null)
        {
            if (copyCallbacks == null) copyCallbacks = new CopyCallbacks();
            if (!Exists(source)) throw new VFSException(string.Format("Source {0} does not exist", source));
            if (Exists(dest)) throw new VFSException(string.Format("Destination {0} already exists", dest));

            CreateParentFolder(dest);
            _fileSystem.Copy(FindNode(source), FindParentFolder(dest), PathParser.GetNodeName(dest), copyCallbacks);
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

        private static string GetPath(IIndexNode node)
        {
            var sb = new StringBuilder();
            while (node.Parent != null)
            {
                sb.Insert(0, node.Name);
                sb.Insert(0, "/");
                node = node.Parent;
            }
            return sb.ToString();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource.

            if (!disposing) return;

            // free managed resources
            if (_fileSystem == null) return;

            _fileSystem.Dispose();
            _fileSystem = null;
        }

        public IEnumerable<string> Folders(string test, long version)
        {
            var latestVersion = _fileSystem.CurrentVersion;
            try
            {
                _fileSystem.SwitchToVersion(version);
                return Folders(test);
            }
            finally
            {
                _fileSystem.SwitchToVersion(latestVersion);
            }
        }

        public long Version(string path)
        {
            if (!Exists(path)) throw new VFSException("does not exist");

            return FindNode(path).Version;
        }

        public IEnumerable<long> Versions(string path)
        {
            ISet<long> versions = new HashSet<long>();
            var originalVersion = Version("/");
            try
            {
                SwitchToLatestVersion();
                var currentVersion = Version("/");

                while (currentVersion >= 0)
                {
                    SwitchToVersion(currentVersion);
                    if (Exists(path)) versions.Add(Version(path));
                    currentVersion -= 1;
                }
            }
            finally
            {
                SwitchToVersion(originalVersion);
            }

            return versions;
        }

        public void SwitchToVersion(long version)
        {
            if (Version("/") < version) SwitchToLatestVersion();
            _fileSystem.SwitchToVersion(version);
        }

        public void SwitchToLatestVersion()
        {
            _fileSystem.SwitchToLatestVersion();
        }

    }
}
