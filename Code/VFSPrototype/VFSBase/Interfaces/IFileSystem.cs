using System.Collections.Generic;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    internal interface IFileSystem
    {
        IFileSystemData FileSystemData { get; }
        IEnumerable<Folder> Folders(Folder folder);
        Folder FindFolder(Folder folder, string folderName);
        void CreateFolder(Folder folder);
        void Import(string source, Folder dest, string nameOfNewElement);
        void Export(IIndexNode source, string dest);
        void Copy(IIndexNode toCopy, Folder dest, string nameOfCopiedElement);
        void Delete(IIndexNode node);
        void Move(IIndexNode toMove, Folder dest, string nameOfMovedElement);
        bool Exists(Folder folder, string nameOfTheElement);
        Folder Root { get; }
    }
}