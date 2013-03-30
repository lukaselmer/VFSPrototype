using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VFSBase
{
    public class Folder : IComparable
    {
        public Folder(string name)
            : this()
        {
            Name = name;
        }

        public Folder()
        {
            Folders = new SortedSet<Folder>();
            Files = new SortedSet<VFSFile>();
        }

        public string Name { get; private set; }

        public ISet<Folder> Folders { get; private set; }

        public ISet<VFSFile> Files { get; private set; } 

        public Folder CreateFolder(Queue<string> folders)
        {
            if (!folders.Any()) return this;

            var folderName = folders.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) // Folder does not exist, create one
            {
                folder = new Folder(folderName);
                Folders.Add(folder);
            }
            return folder.CreateFolder(folders);
        }

        public void DeleteFolder(Queue<string> folders)
        {
            if (!folders.Any()) throw new ArgumentException("Folder cannot be empty");

            if (folders.Count == 1)
            {
                // Found the folder to be deleted
                var folderToBeDeleted = FindFolder(folders.Dequeue());
                if (folderToBeDeleted == null) throw new DirectoryNotFoundException();
                Folders.Remove(folderToBeDeleted);
                return;
            }

            var folderName = folders.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) throw new DirectoryNotFoundException();
            folder.DeleteFolder(folders);
        }

        private Folder FindFolder(string folderName)
        {
            return Folders.FirstOrDefault(f => f.Name == folderName);
        }

        public bool DoesFolderExist(Queue<string> folders)
        {
            if (!folders.Any()) return true;
            var folder = FindFolder(folders.Dequeue());
            return folder != null && folder.DoesFolderExist(folders);
        }

        private VFSFile FindFile(string fileName)
        {
            return Files.FirstOrDefault(f => f.Name == fileName);
        }

        public bool DoesFileExist(Queue<string> path)
        {
            if (path.Count == 1)
            {
                var file = FindFile(path.Dequeue());
                return file != null;
            }

            var folder = FindFolder(path.Dequeue());
            return folder != null && folder.DoesFileExist(path);
        }

        public VFSFile ImportFile(Queue<string> path, string source)
        {
            if (path.Count == 1)
            {
                var file = new VFSFile(path.Dequeue(), source);
                Files.Add(file);
                return file;
            }

            var folderName = path.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null)
            {
                folder = new Folder(folderName);
                Folders.Add(folder);
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


        public void DeleteFile (Queue<string> path)
        {
            if (path.Count == 1)
            {
                var file = FindFile(path.Dequeue());
                if (file == null) throw new FileNotFoundException();
                Files.Remove(file);
                return;
            }

            var folderName = path.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) throw new FileNotFoundException();
            folder.DeleteFile(path);
        }

        public int CompareTo(object obj)
        {
            var folder = obj as Folder;
            if (folder == null) return -1;
            return String.Compare(Name, folder.Name, StringComparison.Ordinal);
        }

    }
}