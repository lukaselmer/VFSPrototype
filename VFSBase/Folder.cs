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
        }

        protected string Name { get; private set; }

        public ISet<Folder> Folders { get; private set; }

        public void CreateFolder(Queue<string> folders)
        {
            if (!folders.Any()) return;

            var folderName = folders.Dequeue();
            var folder = FindFolder(folderName);
            if (folder == null) // Folder does not exist, create one
            {
                folder = new Folder(folderName);
                Folders.Add(folder);
            }
            folder.CreateFolder(folders);
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

        public int CompareTo(object obj)
        {
            var folder = obj as Folder;
            if (folder == null) return -1;
            return String.Compare(Name, folder.Name, StringComparison.Ordinal);
        }

    }
}