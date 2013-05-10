using System;
using System.Windows;

namespace VFSBrowser.ViewModel
{
    internal static class UserMessage
    {
        public static void Information(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Error(string error, string title)
        {
            MessageBox.Show(error, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void Exception(Exception exception)
        {
            Error(exception.Message, "An error occured");
        }
    }
}