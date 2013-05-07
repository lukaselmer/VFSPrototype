namespace VFSWCFContracts.DataTransferObjects
{
    public enum SynchronizationState
    {
        RemoteChanges,
        LocalChanges,
        Conflicted,
        UpToDate
    }
}