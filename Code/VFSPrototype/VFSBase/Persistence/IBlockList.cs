using System.Collections.Generic;
using VFSBase.Interfaces;

namespace VFSBase.Persistence
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
        void Add(long reference);

        /// <summary>
        /// Removes the specified node from the list.
        /// </summary>
        /// <param name="nodeToDelete">The node to delete.</param>
        /// <param name="freeSpace">if set to <c>true</c> [free space].</param>
        void Remove(IIndexNode nodeToDelete, bool freeSpace = true);

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
    }
}
