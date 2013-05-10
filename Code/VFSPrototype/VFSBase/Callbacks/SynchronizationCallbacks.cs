using System;
using VFSBase.DiskServiceReference;

namespace VFSBase.Synchronization
{
    public class SynchronizationCallbacks
    {
        public SynchronizationCallbacks(Action<SynchronizationState> stateChanged, Action<long, long> progressChanged)
        {
            StateChanged = stateChanged ?? ((a) => { });
            ProgressChanged = progressChanged ?? ((a, b) => { });
        }

        public Action<SynchronizationState> StateChanged { get; private set; }
        public Action<long, long> ProgressChanged { get; private set; }
    }
}