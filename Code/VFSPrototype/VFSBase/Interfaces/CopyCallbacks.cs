using System;

namespace VFSBase.Interfaces
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