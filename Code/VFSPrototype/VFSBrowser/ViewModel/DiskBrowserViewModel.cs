using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VFSBase.DiskServiceReference;
using VFSBase.Interfaces;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    public class DiskBrowserViewModel : AbstractViewModel
    {
        private readonly IFileSystemTextManipulator _manipulator;

        private ObservableCollection<DiskDto> _disks;
        public ObservableCollection<DiskDto> Disks
        {
            get { return _disks; }
            set
            {
                _disks = value;
                OnPropertyChanged("Disks");
            }
        }

        public object SelectItemCommand { get; private set; }

        public DiskDto SelectedItem { get; private set; }

        public DiskBrowserViewModel(DiskServiceClient diskService, UserDto user)
        {
            _disks = new ObservableCollection<DiskDto>(diskService.Disks(user));

            SelectItemCommand = new Command(SelectItem, o => SelectedItem != null);
        }

        private void SelectItem(object parameter)
        {
            _dlg.Close();
        }

        private DiskBrowserDialog _dlg;

        public bool ShowDialog()
        {
            _dlg = new DiskBrowserDialog(this);
            return _dlg.ShowDialog();
        }
    }
}
