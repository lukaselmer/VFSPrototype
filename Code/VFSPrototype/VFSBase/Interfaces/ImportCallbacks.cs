using System;

namespace VFSBase.Interfaces
{
    public class ImportCallbacks
    {
        private readonly Func<bool> _shouldAbort;
        private readonly Action<bool> _operationCompleted;
        private readonly Action<int> _totalToProcessChanged;
        private readonly Action<int> _currentlyProcessedChanged;

        private int _totalToProcess = 0;
        private int _currentlyProcessed = 0;

        public ImportCallbacks(Func<bool> shouldAbort = null, Action<bool> operationCompleted = null, Action<int> totalToProcessChanged = null, Action<int> currentlyProcessedChanged = null)
        {
            _shouldAbort = shouldAbort ?? (() => false);
            _operationCompleted = operationCompleted ?? (b => { });
            _totalToProcessChanged = totalToProcessChanged ?? (i => { });
            _currentlyProcessedChanged = currentlyProcessedChanged ?? (i => { });
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

        internal int TotalToProcess
        {
            get { return _totalToProcess; }
            set
            {
                _totalToProcess = value;
                if (TotalToProcessChanged != null) TotalToProcessChanged(_totalToProcess);
            }
        }

        internal int CurrentlyProcessed
        {
            get { return _currentlyProcessed; }
            set
            {
                _currentlyProcessed = value;
                if (CurrentlyProcessedChanged != null) CurrentlyProcessedChanged(_currentlyProcessed);
            }
        }
    }
}