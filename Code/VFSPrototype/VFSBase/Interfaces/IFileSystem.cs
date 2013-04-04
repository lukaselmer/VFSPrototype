using System;
using System.Collections.Generic;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    internal interface IFileSystem : IDisposable
    {
        FileSystemOptions FileSystemOptions { get; }
        IEnumerable<Folder> Folders(Folder folder);
        IIndexNode Find(Folder folder, string name);
        void CreateFolder(Folder parentFolder, string name);
        void Import(string source, Folder dest, string name);
        void Export(IIndexNode source, string dest);
        void Copy(IIndexNode toCopy, Folder dest, string nameOfCopiedElement);
        void Delete(IIndexNode node);
        void Move(IIndexNode toMove, Folder dest, string name);
        bool Exists(Folder folder, string name);
        RootFolder Root { get; }
    }
}