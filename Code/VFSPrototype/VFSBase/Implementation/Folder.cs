using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal class Folder : IComparable, IIndexNode
    {
        public Folder(string name)
            : this()
        {
            Name = name;
        }

        public Folder()
        {
            IndexNodes = new SortedSet<IIndexNode>();
        }

        public string Name { get; set; }

        public ISet<IIndexNode> IndexNodes { get; set; }

        public ISet<Folder> Folders
        {
            get { return new SortedSet<Folder>(IndexNodes.OfType<Folder>()); }
            set { 
                var files = Files;
                IndexNodes.Clear();

                foreach (var folder in value)
                {
                    IndexNodes.Add(folder);
                }
                foreach (var vfsFile in files)
                {
                    IndexNodes.Add(vfsFile);
                }
            }
        }
        public ISet<VFSFile> Files
        {
            get { return new SortedSet<VFSFile>(IndexNodes.OfType<VFSFile>()); }
            set
            {
                var folders = Folders;
                IndexNodes.Clear();

                foreach (var file in value)
                {
                    IndexNodes.Add(file);
                }
                foreach (var folder in folders)
                {
                    IndexNodes.Add(folder);
                }
            }
        }

        public Folder GetFolder(Queue<string> folders)
        {
            if (!folders.Any()) return this;

            var folderName = folders.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) throw new DirectoryNotFoundException();
            return folder.GetFolder(folders);
        }

        public Folder CreateFolder(Queue<string> folders)
        {
            if (!folders.Any()) return this;

            var folderName = folders.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) // Folder does not exist, create one
            {
                folder = new Folder(folderName);
                IndexNodes.Add(folder);
            }
            return folder.CreateFolder(folders);
        }

        public IIndexNode Delete(Queue<string> path)
        {
            if (path.Count == 1)
            {
                var node = Find(path.Dequeue());
                if (node == null) throw new NotFoundException();
                IndexNodes.Remove(node);
                return node;
            }

            var folderName = path.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) throw new NotFoundException();
            return folder.Delete(path);
        }

        public void Insert(Queue<string> path, IIndexNode node)
        {
            if (!path.Any())
            {
                IndexNodes.Add(node);
                return;
            }

            var folderName = path.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null)
            {
                folder = new Folder(folderName);
                IndexNodes.Add(folder);
            }
            folder.Insert(path, node);
        }

        private Folder FindFolder(string folderName)
        {
            return Folders.FirstOrDefault(f => f.Name == folderName);
        }

        private VFSFile FindFile(string fileName)
        {
            return Files.FirstOrDefault(f => f.Name == fileName);
        }

        public IIndexNode Find(string name)
        {
            return IndexNodes.FirstOrDefault(i => i.Name == name);
        }

        public bool Exists(Queue<string> path)
        {
            if (path.Count == 1)
            {
                var node = Find(path.Dequeue());
                return (node != null);
            }

            var name = path.Dequeue();
            var folder = FindFolder(name);
            return folder != null && folder.Exists(path);
        }

        public VFSFile ImportFile(Queue<string> path, string source)
        {
            if (path.Count == 1)
            {
                var file = new VFSFile(path.Dequeue(), source);
                IndexNodes.Add(file);
                return file;
            }

            var folderName = path.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null)
            {
                folder = new Folder(folderName);
                IndexNodes.Add(folder);
            }
            return folder.ImportFile(path, source);
        }


        public void ExportFile(Queue<string> path, string dest)
        {
            if (path.Count == 1)
            {
                var file = FindFile(path.Dequeue());
                if (file == null) throw new FileNotFoundException();
                File.WriteAllBytes(dest, file.Data);
                return;
            }

            var folderName = path.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) throw new FileNotFoundException();
            folder.ExportFile(path, dest);
        }

        public int CompareTo(object obj)
        {
            var folder = obj as Folder;
            if (folder == null) return -1;
            return String.Compare(Name, folder.Name, StringComparison.Ordinal);
        }

    }
}