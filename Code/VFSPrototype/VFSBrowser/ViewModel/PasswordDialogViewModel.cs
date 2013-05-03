using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    internal class PasswordDialogViewModel : AbstractViewModel
    {
        
        private PasswordDialog _dlg;

        public Command OkCommand { get; private set; }

        public PasswordDialogViewModel()
        {
            OkCommand = new Command(Ok, p => true);
        }

        private void Ok(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) throw new VFSException("Something went wrong with the password");
            Password = passwordBox.Password;

            _dlg.DialogResult = true;
            _dlg.Close();
        }

        public string Password { get; private set; }

        public bool? ShowDialog()
        {
            _dlg = new PasswordDialog(this);
            return _dlg.ShowDialog();
        }
    }
}
