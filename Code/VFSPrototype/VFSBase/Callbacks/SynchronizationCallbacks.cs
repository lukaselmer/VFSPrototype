using System;
using VFSBase.DiskServiceReference;

namespace VFSBase.Synchronization
{
    public class SynchronizationCallbacks
    {
        public SynchronizationCallbacks(Action<SynchronizationState> stateChanged)
        {
            StateChanged = stateChanged ?? (s => { });
        }

        public Action<SynchronizationState> StateChanged { get; private set; }
    }
}