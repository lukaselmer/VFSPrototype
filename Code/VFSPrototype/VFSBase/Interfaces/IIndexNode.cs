using System.Collections.Generic;

namespace VFSBase.Interfaces
{
    public interface IIndexNode
    {
        string Name { get; set; }
        IList<IIndexNode> IndexNodes { get; set; }
    }
}