using System.Collections.Generic;

namespace VFSBase
{
    public interface IIndexNode
    {
        string Name { get; }
        ISet<IIndexNode> IndexNodes { get; set; }
    }
}