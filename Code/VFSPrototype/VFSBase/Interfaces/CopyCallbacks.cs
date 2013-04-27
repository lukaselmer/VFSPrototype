using System;

namespace VFSBase.Interfaces
{
    public class CopyCallbacks
    {
        private readonly Func<bool> _shouldAbort;
        private readonly Action<bool> _operationCompleted;

        public CopyCallbacks(Func<bool> shouldAbort = null, Action<bool> operationCompleted = null)
        {
            _shouldAbort = shouldAbort;
            _operationCompleted = operationCompleted;
        }

        public Func<bool> ShouldAbort
        {
            get { return _shouldAbort; }
        }

        public Action<bool> OperationCompleted
        {
            get { return _operationCompleted; }
        }
    }
}