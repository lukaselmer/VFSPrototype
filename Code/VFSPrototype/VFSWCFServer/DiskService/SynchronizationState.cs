using System.Runtime.Serialization;

namespace VFSWCFService.DiskService
{
    public enum SynchronizationState
    {
        RemoteChanges,
        LocalChanges,
        Conflicted,
        UpToDate
    }
}