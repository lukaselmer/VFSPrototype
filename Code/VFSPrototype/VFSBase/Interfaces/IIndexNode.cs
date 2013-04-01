using System.Collections.Generic;

namespace VFSBase.Interfaces
{
    public interface IIndexNode
    {
        string Name { get; set; }
        ISet<IIndexNode> IndexNodes { get; set; }
    }
}