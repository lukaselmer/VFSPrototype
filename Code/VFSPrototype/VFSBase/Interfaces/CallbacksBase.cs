using System;
using System.Diagnostics;

namespace VFSBase.Interfaces
{
    /// <summary>
    /// The callbacks base
    /// </summary>
    public abstract class CallbacksBase
    {
        /// <summary>
        /// The should abort function is called multiple times during the import.
        /// This way, the import can be aborted.
        /// </summary>
        private readonly Func<bool> _shouldAbort;

        /// <summary>
        /// The operation completed action is called when the operation completes.
        /// The bool indicates wether the operation was successful or not.
        /// </summary>
        private readonly Action<bool> _operationCompleted;

        /// <summary>
        /// The total to process changed action is called when the total count has changed.
        /// This way, a status progress can be displayed.
        /// </summary>
        private readonly Action<int> _totalToProcessChanged;

        /// <summary>
        /// The currently processed changed action is called when the currently processed count has changed.
        /// This way, a status progress can be displayed.
        /// </summary>
        private readonly Action<int> _currentlyProcessedChanged;

        /// <summary>
        /// The total to process count
        /// </summary>
        private int _totalToProcess;

        /// <summary>
        /// The currently processed count
        /// </summary>
        private int _currentlyProcessed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbacksBase"/> class.
        /// </summary>
        /// <param name="shouldAbort">The should abort function is called multiple times during the import.</param>
        /// <param name="operationCompleted">The operation completed action is called when the operation completes.</param>
        /// <param name="totalToProcessChanged">The total to process changed action is called when the total count has changed.</param>
        /// <param name="currentlyProcessedChanged">The currently processed changed action is called when the currently processed count has changed.</param>
        protected CallbacksBase(Func<bool> shouldAbort, Action<bool> operationCompleted, Action<int> totalToProcessChanged, Action<int> currentlyProcessedChanged)
        {
            _shouldAbort = shouldAbort ?? (() => false);
            _operationCompleted = operationCompleted ?? (b => { });
            _totalToProcessChanged = totalToProcessChanged ?? (i => { });
            _currentlyProcessedChanged = currentlyProcessedChanged ?? (i => { });
        }

        /// <summary>
        /// Gets the should abort function.
        /// </summary>
        /// <value>
        /// The should abort function.
        /// </value>
        internal Func<bool> ShouldAbort
        {
            get { return _shouldAbort; }
        }

        /// <summary>
        /// Gets the operation completed action.
        /// </summary>
        /// <value>
        /// The operation completed action.
        /// </value>
        internal Action<bool> OperationCompleted
        {
            get { return _operationCompleted; }
        }

        /// <summary>
        /// Gets the total to process changed action.
        /// </summary>
        /// <value>
        /// The total to process changed action.
        /// </value>
        private Action<int> TotalToProcessChanged
        {
            get { return _totalToProcessChanged; }
        }

        /// <summary>
        /// Gets the currently processed changed action.
        /// </summary>
        /// <value>
        /// The currently processed changed action.
        /// </value>
        private Action<int> CurrentlyProcessedChanged
        {
            get { return _currentlyProcessedChanged; }
        }

        /// <summary>
        /// Gets or sets the total to process count.
        /// </summary>
        /// <value>
        /// The total to process count.
        /// </value>
        internal int TotalToProcess
        {
            get { return _totalToProcess; }
            set
            {
                _totalToProcess = value;
                if (TotalToProcessChanged != null) TotalToProcessChanged(_totalToProcess);
            }
        }

        /// <summary>
        /// Gets or sets the currently processed count.
        /// </summary>
        /// <value>
        /// The currently processed count.
        /// </value>
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