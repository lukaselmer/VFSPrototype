using System;
using VFSBase.DiskServiceReference;

namespace VFSBase.Callbacks
{
    public class SynchronizationCallbacks
    {
        public SynchronizationCallbacks(Action finished, Action<long, long> progressChanged)
        {
            Finished = finished ?? (() => { });
            ProgressChanged = progressChanged ?? ((a, b) => { });
        }

        public Action Finished { get; private set; }
        public Action<long, long> ProgressChanged { get; private set; }
    }
}