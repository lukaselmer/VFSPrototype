using System;
using System.Timers;
using VFSBase.Callbacks;
using VFSBase.DiskServiceReference;
using VFSBase.Exceptions;
using VFSBase.Interfaces;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    internal class SynchronizationViewModel : AbstractViewModel
    {
        private Timer _synchronizationTimer;

        private readonly SynchronizationDialog _dlg;
        private readonly ISynchronizationService _service;

        public SynchronizationViewModel(IFileSystemTextManipulator manipulator, UserDto user, Action synchronizationFinished)
        {
            _service = manipulator.GenerateSynchronizationService(
                user, new SynchronizationCallbacks(synchronizationFinished, SynchronizationProgrssChanged)); ;
            _dlg = new SynchronizationDialog(this);
            _dlg.Hide();

            InitSynchronizationTimer();
        }

        private long _currentlyProcessed;
        public long CurrentlyProcessed
        {
            get { return _currentlyProcessed; }
            set
            {
                _currentlyProcessed = value;
                OnPropertyChanged("CurrentlyProcessed");
            }
        }

        private long _totalToProcess;
        public long TotalToProcess
        {
            get { return _totalToProcess; }
            set
            {
                _totalToProcess = value;
                OnPropertyChanged("TotalToProcess");
            }
        }

        public bool Closed { get; private set; }

        private void SynchronizationProgrssChanged(long done, long total)
        {
            CurrentlyProcessed = done;
            TotalToProcess = total;
        }

        public void ShowDialog()
        {
            if (_service == null) return;

            Closed = false;
            _dlg.ShowDialog();
        }

        public void Hide()
        {
            Closed = true;
            _dlg.Hide();
        }


        private void InitSynchronizationTimer()
        {
            if (_synchronizationTimer != null) throw new VFSException("synchronization timer should be null!");

            _synchronizationTimer = new Timer(3000) { AutoReset = true, Enabled = true, Interval = 3000 };
            _synchronizationTimer.Elapsed += CheckSynchronization;
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CheckSynchronization(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_synchronizationTimer != null) _synchronizationTimer.Dispose();
            _synchronizationTimer = null;

            if (_service == null) return;

            try
            {
                var state = _service.FetchSynchronizationState();

                if (state == SynchronizationState.Conflicted) ViewModelHelper.InvokeOnGuiThread(HandleConflictedState);
                if (state == SynchronizationState.LocalChanges || state == SynchronizationState.RemoteChanges) ViewModelHelper.InvokeOnGuiThread(Synchronize);

                if (state == SynchronizationState.UpToDate || state == SynchronizationState.Conflicted) ViewModelHelper.InvokeOnGuiThread(Hide);
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }

            InitSynchronizationTimer();
        }


        private void HandleConflictedState()
        {
            _synchronizationTimer.Dispose();
            _synchronizationTimer = null;
            const string message = "There was a conflict during synchronization, and therefore the " +
                                   "online mode has been disabled. Please back up your changes, roll " +
                                   "back your local version, and restart switch back to the online mode.";
            UserMessage.Error(message, "Synchronization stopped");
        }

        private void Synchronize()
        {
            ViewModelHelper.RunAsyncAction(() => _service.Synchronize(), task => Hide());
            ShowDialog();
        }

        public void StopSynchronization()
        {
            if (_synchronizationTimer != null)
            {
                _synchronizationTimer.Enabled = false;
                _synchronizationTimer.Dispose();
                _synchronizationTimer = null;
            }
        }
    }
}
