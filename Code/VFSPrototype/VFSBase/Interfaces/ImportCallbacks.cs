using System;

namespace VFSBase.Interfaces
{
    public class ImportCallbacks : CallbacksBase
    {
        public ImportCallbacks() : base(null, null, null, null) { }

        public ImportCallbacks(Func<bool> shouldAbort, Action<bool> operationCompleted) : base(shouldAbort, operationCompleted, null, null) { }

        public ImportCallbacks(Func<bool> shouldAbort, Action<bool> operationCompleted, Action<int> totalToProcessChanged, Action<int> currentlyProcessedChanged)
            : base(shouldAbort, operationCompleted, totalToProcessChanged, currentlyProcessedChanged)
        {
        }
    }
}