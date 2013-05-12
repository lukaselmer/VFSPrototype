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
    internal class LoginDialogViewModel : AbstractViewModel
    {
        private LoginDialog _dlg;
        private string _login;
        private string _password;

        public Command OkCommand { get; private set; }
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged("Login");
            }
        }
        public LoginDialogViewModel()
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

        public string Password
        {
            get { return _password ?? ""; }
            private set { _password = value; }
        }

        public bool? ShowDialog()
        {
            _dlg = new LoginDialog(this);
            return _dlg.ShowDialog();
        }
    }
}
