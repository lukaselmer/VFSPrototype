using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    internal interface IIndexNode
    {
        string Name { get; set; }
        Folder Parent { get; set; }
        long BlockNumber { get; set; }
        long IndirectNodeNumber { get; set; }
        long BlocksCount { get; set; }
        long Version { get; }
        long PredecessorBlockNr { get; set; }
    }
}
