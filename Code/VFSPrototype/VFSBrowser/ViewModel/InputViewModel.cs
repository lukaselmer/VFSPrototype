using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    public class InputViewModel : AbstractViewModel
    {
        private string _text;
        public string Text { 
            get { return _text; } 
            set { 
                _text = value;
                OnPropertyChanged("Text");
            } 
        }

        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged("Label");
            }
        }

        public InputViewModel(string label, string text)
        {
            _label = label;
            _text = text;
        }

        private InputDialog _dlg;
        public bool? ShowDialog()
        {
            _dlg = new InputDialog(this);
            return _dlg.ShowDialog();
        }

        private ICommand _okCommand;
        public ICommand OkCommand { get { return _okCommand ?? (_okCommand = new Command(CloseDialog, p => (!string.IsNullOrEmpty(Text)))); } }

        private void CloseDialog(object p)
        {
            _dlg.DialogResult = true;
            _dlg.Close();
        }
    }
}
