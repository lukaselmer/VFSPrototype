using System;
using System.Collections.Generic;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    internal interface IFileSystem : IDisposable
    {
        /// <summary>
        /// Enumerates all files and folders of the specified folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>IEnumerable{Folder}.</returns>
        IEnumerable<IIndexNode> AsEnumerable(Folder folder);

        /// <summary>
        /// Enumerates all folders of the specified folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>IEnumerable{Folder}.</returns>
        IEnumerable<Folder> Folders(Folder folder);

        /// <summary>
        /// Determines whether a file or folder with a specified name exists in a specified folder.
        /// </summary>
        /// <param name="folder">The folder where to search.</param>
        /// <param name="name">The name of the file or folder to search for.</param>
        /// <returns><c>true</c> if file or folder with name exists, <c>false</c> otherwise</returns>
        bool Exists(Folder folder, string name);

        /// <summary>
        /// Finds the element with a specified name in the specified folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="name">The name of the file or folder to search for.</param>
        /// <returns>The IIndexNode if found, <c>null</c> otherwise.</returns>
        IIndexNode Find(Folder folder, string name);

        /// <summary>
        /// Creates a new folder with a specified name in the folder parent folder.
        /// </summary>
        /// <param name="parentFolder">The parent folder.</param>
        /// <param name="name">The name of the new folder.</param>
        /// <returns>The newly created folder.</returns>
        /// <exception cref=""></exception>
        Folder CreateFolder(Folder parentFolder, string name);
        void Import(string source, Folder dest, string name);
        void Export(IIndexNode source, string dest);
        void Copy(IIndexNode toCopy, Folder dest, string nameOfCopiedElement);
        void Delete(IIndexNode node);
        void Move(IIndexNode toMove, Folder dest, string name);
        RootFolder Root { get; }

        /// <summary>
        /// Gets the file system options.
        /// </summary>
        /// <value>The file system options.</value>
        FileSystemOptions FileSystemOptions { get; }

    }
}