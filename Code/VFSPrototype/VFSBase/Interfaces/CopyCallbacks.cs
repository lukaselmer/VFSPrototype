using System;

namespace VFSBase.Interfaces
{
    public class CopyCallbacks : CallbacksBase
    {
        public CopyCallbacks(Func<bool> shouldAbort = null, Action<bool> operationCompleted = null)
            : base(shouldAbort, operationCompleted, null, null)
        {
        }
    }
}