using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VFSBase.Interfaces;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    public class DiskInfoViewModel : AbstractViewModel
    {
        private readonly IFileSystemTextManipulator _manipulator;

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        private string _freeDiskSpace;
        public string FreeDiskSpace
        {
            get { return _freeDiskSpace; }
            set
            {
                _freeDiskSpace = value;
                OnPropertyChanged("FreeDiskSpace");
            }
        }

        private string _freeDiskSpaceGb;
        public string FreeDiskSpaceGb
        {
            get { return _freeDiskSpaceGb; }
            set
            {
                _freeDiskSpaceGb = value;
                OnPropertyChanged("FreeDiskSpaceGb");
            }
        }

        private string _occupiedDiskSpace;
        public string OccupiedDiskSpace
        {
            get { return _occupiedDiskSpace; }
            set
            {
                _occupiedDiskSpace = value;
                OnPropertyChanged("OccupiedDiskSpace");
            }
        }

        private string _occupiedDiskSpaceGb;
        public string OccupiedDiskSpaceGb
        {
            get { return _occupiedDiskSpaceGb; }
            set
            {
                _occupiedDiskSpaceGb = value;
                OnPropertyChanged("OccupiedDiskSpaceGb");
            }
        }

        public DiskInfoViewModel(IFileSystemTextManipulator manipulator)
        {
            _manipulator = manipulator;

            _filePath = _manipulator.FileSystemOptions.Location;
            _freeDiskSpace = String.Format("{0:0,0} Bytes", _manipulator.FileSystemOptions.DiskFree);
            _freeDiskSpaceGb = String.Format("{0:0,0.000} GB", _manipulator.FileSystemOptions.DiskFree / 1024.0 / 1024.0 / 1024.0);
            _occupiedDiskSpace = String.Format("{0:0,0} Bytes", _manipulator.FileSystemOptions.DiskOccupied);
            _occupiedDiskSpaceGb = String.Format("{0:0,0.000} GB", _manipulator.FileSystemOptions.DiskOccupied / 1024.0 / 1024.0 / 1024.0);
        }

        private DiskInfoDialog _dlg;

        public void ShowDialog()
        {
            _dlg = new DiskInfoDialog(this);
            _dlg.ShowDialog();
        }
    }
}
