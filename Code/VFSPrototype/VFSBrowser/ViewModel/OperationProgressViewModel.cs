using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VFSBase.Callbacks;
using VFSBase.Interfaces;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    internal class OperationProgressViewModel : AbstractViewModel
    {
        private int _currentlyProcessed;
        public int CurrentlyProcessed
        {
            get { return _currentlyProcessed; }
            set
            {
                _currentlyProcessed = value;
                OnPropertyChanged("CurrentlyProcessed");
            }
        }

        private int _totalToProcess;
        public int TotalToProcess
        {
            get { return _totalToProcess; }
            set
            {
                _totalToProcess = value;
                OnPropertyChanged("TotalToProcess");
            }
        }

        private OperationProgressView _dlg;
        private bool _shouldAbort;

        public CallbacksBase Callbacks { get; private set; }

        public Command AbortCommand { get; private set; }

        public OperationProgressViewModel()
        {
            Callbacks = new ExportCallbacks(() => _shouldAbort, OperationCompleted, TotalToProcessChanged, CurrentlyProcessedChanged);
            AbortCommand = new Command(p => _shouldAbort = true, p => (_shouldAbort == false));
        }

        private void CurrentlyProcessedChanged(int amount)
        {
            CurrentlyProcessed = amount;
        }

        private void TotalToProcessChanged(int amount)
        {
            TotalToProcess = amount;
        }

        private void OperationCompleted(bool successful)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                                   {
                                       MessageBox.Show(_dlg, successful ? "Operation completed sucessfully." : "Operation aborted."); _dlg.Close();
                                   }));
        }

        public void ShowDialog()
        {
            _dlg = new OperationProgressView(this);
            _dlg.ShowDialog();
        }
    }
}
