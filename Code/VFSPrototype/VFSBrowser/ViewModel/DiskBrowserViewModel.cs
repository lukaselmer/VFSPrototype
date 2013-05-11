using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VFSBase.DiskServiceReference;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    public class DiskBrowserViewModel : AbstractViewModel
    {
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

        public DiskDto SelectedDisk { get; private set; }

        public string SelectedLocation { get; private set; }

        public DiskBrowserViewModel(DiskServiceClient diskService, UserDto user)
        {
            _disks = new ObservableCollection<DiskDto>(diskService.Disks(user));

            SelectItemCommand = new Command(SelectItem, o => SelectedDisk != null);
        }

        private void SelectItem(object parameter)
        {
            _dlg.DialogResult = true;
            _dlg.Close();
        }

        private DiskBrowserDialog _dlg;

        public bool ShowDialog()
        {
            _dlg = new DiskBrowserDialog(this);

            bool? showDialog = _dlg.ShowDialog();
            if (showDialog != true) return false;

            SelectedLocation = ViewModelHelper.ChoosePlaceForNewVFSFile();
            return SelectedLocation != null;
        }
    }
}
