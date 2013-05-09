using System;

namespace VFSBase.Callbacks
{
    public class CopyCallbacks : CallbacksBase
    {
        
        public CopyCallbacks() : base(null, null, null, null) { }

        public CopyCallbacks(Func<bool> shouldAbort, Action<bool> operationCompleted) : base(shouldAbort, operationCompleted, null, null) { }

        public CopyCallbacks(Func<bool> shouldAbort, Action<bool> operationCompleted, Action<int> totalToProcessChanged, Action<int> currentlyProcessedChanged)
            : base(shouldAbort, operationCompleted, totalToProcessChanged, currentlyProcessedChanged)
        {
        }
    }
}