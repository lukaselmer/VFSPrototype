using VFSBase.DiskServiceReference;

namespace VFSBase.Interfaces
{
    public interface ISynchronizationService
    {
        void Synchronize();
        SynchronizationState FetchSynchronizationState();
    }
}