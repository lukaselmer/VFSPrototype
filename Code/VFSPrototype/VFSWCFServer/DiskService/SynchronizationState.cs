using System.Runtime.Serialization;

namespace VFSWCFService.DiskService
{
    [DataContract]
    public enum SynchronizationState
    {
        RemoteChanges,
        LocalChanges,
        Conflicted,
        UpToDate
    }
}