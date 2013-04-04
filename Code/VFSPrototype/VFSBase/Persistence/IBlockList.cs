using System.Collections.Generic;
using VFSBase.Interfaces;

namespace VFSBase.Persistence
{
    /// <summary>
    /// The Interface IBlockList abstracts away, how the (file or directory) contents/blocks are stored
    /// on the disk.
    /// 
    /// For good performance, this can be implemented as AVL tree or as B+Tree
    /// </summary>
    internal interface IBlockList
    {
        void Add(long reference);
        void Remove(IIndexNode nodeToDelete);
        IEnumerable<IIndexNode> AsEnumerable();
        bool Exists(string name);
        IIndexNode Find(string name);
    }
}