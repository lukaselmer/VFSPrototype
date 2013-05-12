using System.Windows.Controls;
using VFSBase.Exceptions;
using VFSBase.Persistence.Coding.General;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    internal class NewVFSViewModel : AbstractViewModel
    {
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
        private string _password;

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

        public string Password
        {
            get { return _password ?? ""; }
            private set { _password = value; }
        }

        public bool? ShowDialog()
        {
            _dlg = new NewVFSView(this);
            return _dlg.ShowDialog();
        }
    }
}
