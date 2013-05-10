using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace VFSBrowser.ViewModel
{
    internal static class ViewModelHelper
    {

        public static void RunAsyncAction(Action action)
        {
            RunAsyncAction(action, task => { });
        }

        public static void RunAsyncAction(Action action, Action<Task> continueWith)
        {
            Action<Task> composition = task =>
                                           {
                                               if (task.Exception != null) UserMessage.Exception(task.Exception.InnerException);
                                               continueWith(task);
                                           };

            Task.Run(action).ContinueWith(composition, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void InvokeOnGuiThread(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }
    }
}