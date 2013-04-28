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
    public class NewVFSViewModel : AbstractViewModel
    {
        private long _maximumSize;
        public long MaximumSize
        {
            get { return _maximumSize; }
            set
            {
                _maximumSize = value;
                OnPropertyChanged("MaximumSize");
            }
        }

        private StreamCompressionType _compressionType;
        public StreamCompressionType CompressionType
        {
            get { return _compressionType; }
            set
            {
                _compressionType = value;
                OnPropertyChanged("CompressionType");
            }
        }

        private StreamEncryptionType _encryptionType;
        public StreamEncryptionType EncryptionType
        {
            get { return _encryptionType; }
            set
            {
                _encryptionType = value;
                OnPropertyChanged("EncryptionType");
            }
        }



        private NewVFSView _dlg;

        public Command OkCommand { get; private set; }

        public NewVFSViewModel()
        {
            OkCommand = new Command(Ok, p => true);
            CompressionType = StreamCompressionType.None;
            EncryptionType = StreamEncryptionType.None;
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
            _dlg = new NewVFSView(this);
            return _dlg.ShowDialog();
        }
    }
}
