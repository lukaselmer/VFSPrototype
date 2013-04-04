using System;
using System.Collections.Generic;
using VFSBase.Exceptions;
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
        /// <exception cref="AlreadyExistsException">If the folder with <paramref name="name"/> already exists</exception>
        Folder CreateFolder(Folder parentFolder, string name);

        /// <summary>
        /// Imports the specified source (file or folder) from the host system recursively into the folder <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="name">The name.</param>
        void Import(string source, Folder destination, string name);

        /// <summary>
        /// Exports the specified source (file or folder) recursively to the specified destination on the host file system.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        void Export(IIndexNode source, string destination);

        /// <summary>
        /// Copies the specified node recursively to the destination folder.
        /// </summary>
        /// <param name="node">The node to copy.</param>
        /// <param name="destination">The destination where to copy the node to.</param>
        /// <param name="nameOfCopiedElement">The name of the copied element.</param>
        /// TODO: implement this
        void Copy(IIndexNode node, Folder destination, string nameOfCopiedElement);

        /// <summary>
        /// Deletes the specified node recursively.
        /// </summary>
        /// <param name="node">The node to be deleted.</param>
        void Delete(IIndexNode node);

        /// <summary>
        /// Moves the specified node to the destination folder.
        /// </summary>
        /// <param name="node">To move.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="name">The new name of the node (can be the same as node.Name).</param>
        void Move(IIndexNode node, Folder destination, string name);

        /// <summary>
        /// Gets the root folder.
        /// </summary>
        /// <value>The root.</value>
        RootFolder Root { get; }

        /// <summary>
        /// Gets the file system options.
        /// </summary>
        /// <value>The file system options.</value>
        FileSystemOptions FileSystemOptions { get; }
    }
}