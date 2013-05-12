using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VFSBase.DiskServiceReference;
using VFSBrowser.Annotations;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    [UsedImplicitly]
    public class DiskBrowserViewModel : AbstractViewModel
    {
        private readonly ObservableCollection<DiskDto> _disks;

        public ObservableCollection<DiskDto> Disks
        {
            get { return _disks; }
        }

        public object SelectItemCommand { get; private set; }

        public DiskDto SelectedDisk { get; set; }

        public string SelectedLocation { get; private set; }

        public DiskBrowserViewModel(IDiskService diskService, UserDto user)
        {
            if (diskService == null) throw new ArgumentNullException("diskService");
            if (user == null) throw new ArgumentNullException("user");

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
