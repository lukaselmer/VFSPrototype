using System.Collections.Generic;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    public interface IIndexNode
    {
        string Name { get; set; }
        Folder Parent { get; set; }
        //IList<IIndexNode> IndexNodes { get; set; }
    }
}