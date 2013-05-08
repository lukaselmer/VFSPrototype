using System;

namespace VFSBase.Interfaces
{
    public class ExportCallbacks : CallbacksBase
    {

        public ExportCallbacks() : base(null, null, null, null) { }

        public ExportCallbacks(Func<bool> shouldAbort, Action<bool> operationCompleted) : base(shouldAbort, operationCompleted, null, null) { }

        public ExportCallbacks(Func<bool> shouldAbort, Action<bool> operationCompleted, Action<int> totalToProcessChanged, Action<int> currentlyProcessedChanged)
            : base(shouldAbort, operationCompleted, totalToProcessChanged, currentlyProcessedChanged)
        {
        }
    }
}