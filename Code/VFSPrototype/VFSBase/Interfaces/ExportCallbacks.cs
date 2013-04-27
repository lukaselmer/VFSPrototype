using System;

namespace VFSBase.Interfaces
{
    public class ExportCallbacks : CallbacksBase
    {
        public ExportCallbacks(Func<bool> shouldAbort = null, Action<bool> operationCompleted = null, Action<int> totalToProcessChanged = null, Action<int> currentlyProcessedChanged = null)
            : base(shouldAbort, operationCompleted, totalToProcessChanged, currentlyProcessedChanged)
        {
        }
    }
}