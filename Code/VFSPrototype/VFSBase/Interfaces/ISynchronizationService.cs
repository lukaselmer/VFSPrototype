using System;
using VFSBase.DiskServiceReference;

namespace VFSBase.Interfaces
{
    public interface ISynchronizationService : IDisposable
    {
        void Synchronize();
        SynchronizationState FetchSynchronizationState();
    }
}