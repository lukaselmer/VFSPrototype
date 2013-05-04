using System.Collections.Generic;
using System.IO;
using VFSBase.Implementation;
using VFSBase.Persistence.Coding;

namespace VFSBase.Interfaces
{
    /// <summary>
    /// The Interface IBlockList abstracts away how the (file or folder) contents/blocks are stored
    /// on the disk.
    /// 
    /// For good performance, this can be implemented as B-Tree or AVL tree for folders.
    /// See also: http://stackoverflow.com/questions/2734692/avl-tree-vs-b-tree
    /// </summary>
    internal interface IBlockList
    {
        /// <summary>
        /// Adds the specified reference to the list.
        /// </summary>
        /// <param name="reference">The reference.</param>
        void AddReference(long reference);

        /// <summary>
        /// Enumerates through the nodes.
        /// </summary>
        /// <returns>IEnumerable{IIndexNode}.</returns>
        IEnumerable<IIndexNode> AsEnumerable();

        /// <summary>
        /// Looks up, if the element with that name exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool Exists(string name);

        /// <summary>
        /// Finds the specified name in the list.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IIndexNode.</returns>
        IIndexNode Find(string name);

        /// <summary>
        /// Enumerates (lazily) through the blocks.
        /// </summary>
        /// <returns>IEnumerable{System.Byte[]}.</returns>
        IEnumerable<byte[]> Blocks();

        /// <summary>
        /// Copies the toCopy index node, and replaces the toReplace node with the replacement
        /// </summary>
        /// <param name="toCopy">To index node to copy.</param>
        /// <param name="toReplace">To node to be replaced. Can be set to null if only a node should be appended and no one should be replaced.</param>
        /// <param name="replacement">The node to replace the node toReplace. Can be set to null for the delete action.</param>
        /// <param name="newVersion"></param>
        /// <returns></returns>
        Folder CopyReplacingReference(Folder toCopy, IIndexNode toReplace, IIndexNode replacement, long newVersion);
    }
}
