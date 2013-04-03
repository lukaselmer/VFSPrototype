using System.Collections.Generic;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    internal interface IIndexNode
    {
        string Name { get; set; }
        Folder Parent { get; set; }
        long BlockNumber { get; set; }
    }
}