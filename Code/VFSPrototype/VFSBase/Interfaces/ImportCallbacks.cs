using System;

namespace VFSBase.Interfaces
{
    public class ImportCallbacks
    {
        private readonly Func<bool> _shouldAbort;
        private readonly Action<bool> _operationCompleted;
        private readonly Action<int> _totalToProcessChanged;
        private readonly Action<int> _currentlyProcessedChanged;

        public ImportCallbacks(Func<bool> shouldAbort = null, Action<bool> operationCompleted = null, Action<int> totalToProcessChanged = null, Action<int> currentlyProcessedChanged = null)
        {
            _shouldAbort = shouldAbort;
            _operationCompleted = operationCompleted;
            _totalToProcessChanged = totalToProcessChanged;
            _currentlyProcessedChanged = currentlyProcessedChanged;
        }

        public Func<bool> ShouldAbort
        {
            get { return _shouldAbort; }
        }

        public Action<bool> OperationCompleted
        {
            get { return _operationCompleted; }
        }

        public Action<int> TotalToProcessChanged
        {
            get { return _totalToProcessChanged; }
        }

        public Action<int> CurrentlyProcessedChanged
        {
            get { return _currentlyProcessedChanged; }
        }
    }
}