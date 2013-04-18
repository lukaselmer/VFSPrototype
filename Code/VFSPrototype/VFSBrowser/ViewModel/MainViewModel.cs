using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using VFSBase.Implementation;

namespace VFSBrowser.ViewModel
{
    class MainViewModel : AbstractViewModel
    {
        private FileSystemTextManipulator _manipulator;

        private readonly ListItem _parent = new ListItem(null, "..", true);
        public ListItem Parent
        {
            get { return _parent; }
        }

        private String _currentPath;
        public String CurrentPath
        {
            get { return _currentPath; } 
            set
            {
                _currentPath = value;
                Items.Clear();
                Items.Add(Parent);
                foreach (var name in _manipulator.List(CurrentPath))
                {
                    Items.Add(new ListItem(_currentPath, name, _manipulator.IsDirectory(_currentPath + name)));
                }
                OnPropertyChanged("CurrentPath");
                
            }
        }

        public ObservableCollection<ListItem> Items { get; set; }

        public MainViewModel()
        {
            Items = new ObservableCollection<ListItem>();

            OpenVfsCommand = new Command(OpenVfs, null);
            NewVfsCommand = new Command(NewVfs, null);
            NewFolderCommand = new Command(NewFolder, p => (_manipulator != null));
            OpenFolderCommand = new Command(OpenFolder, p => (_manipulator != null && p != null && ((ListItem)p).IsDirectory));
            RenameCommand = new Command(Rename, p => (_manipulator != null && p != null));
            ImportFileCommand = new Command(ImportFile, p => (_manipulator != null));
            ExportFileCommand = new Command(Export, p => (_manipulator != null && p != null));
            DeleteCommand = new Command(Delete, p => (_manipulator != null && p != null));
            MoveCommand = new Command(Move, p => (_manipulator != null && p != null));
            CopyCommand = new Command(Copy, p => (_manipulator != null && p != null));
            PasteCommand = new Command(Paste, p => (_manipulator != null));
        }


        public Command OpenVfsCommand { get; private set; }
        public Command NewVfsCommand { get; private set; }
        public Command NewFolderCommand { get; private set; }
        public Command OpenFolderCommand { get; private set; }
        public Command RenameCommand { get; private set; }
        public Command ImportFileCommand { get; private set; }
        public Command ExportFileCommand { get; private set; }
        public Command CopyCommand { get; private set; }
        public Command MoveCommand { get; private set; }
        public Command PasteCommand { get; private set; }
        public Command DeleteCommand { get; private set; }


        private void Delete(object parameter)
        {
            var deleteItems = parameter as ObservableCollection<object>;

            if (deleteItems == null)
                return;

            var del = new List<ListItem>();
            foreach (ListItem item in deleteItems)
            {
                _manipulator.Delete(CurrentPath + item.Name);
                var listItem = Items.First(l => l.Name == item.Name);
                del.Add(listItem);
            }

            del.ForEach(i => Items.Remove(i));
        }

        private bool _copy;
        private readonly List<ListItem> _clipboard = new List<ListItem>(); 
        
        private void Copy(object parameter)
        {
            var items = parameter as ObservableCollection<object>;

            if (items == null)
                return;

            _clipboard.Clear();
            _copy = true;
            foreach (ListItem item in items)
                _clipboard.Add(item);
        }

        private void Move(object parameter)
        {
            var items = parameter as ObservableCollection<object>;

            if (items == null)
                return;

            _clipboard.Clear();
            _copy = false;
            foreach (ListItem item in items)
                _clipboard.Add(item);
        }

        private void Paste(object parameter)
        {
            foreach (var source in _clipboard)
            {
                if (_manipulator.Exists(CurrentPath + source.Name))
                {
                    var result = System.Windows.MessageBox.Show("Replace file?", "File allready exists!", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.No)
                        continue;
                    _manipulator.Delete(CurrentPath + source.Name);
                    var listItem = Items.First(l => l.Name == source.Name);
                    Items.Remove(listItem);
                }

                if (_copy)
                    _manipulator.Copy(source.Path + source.Name, CurrentPath + source.Name);
                else
                    _manipulator.Move(source.Path + source.Name, CurrentPath + source.Name);

                Items.Add(new ListItem(CurrentPath, source.Name, _manipulator.IsDirectory(CurrentPath + source.Name)));
            }
        }

        private void Export(object parameter)
        {
            var items = parameter as ObservableCollection<object>;

            if (items == null)
                return;

            var dlg = new FolderBrowserDialog() { ShowNewFolderButton = true };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (ListItem item in items)
                    _manipulator.Export(CurrentPath + item.Name, dlg.SelectedPath + "\\" + item.Name);
            }
        }

        private void Rename(object parameter)
        {
            var item = parameter as ListItem;

            if (item == null)
                return;

            var dlg = new InputViewModel("New Name", item.Name);
            var result = dlg.ShowDialog();
          
            if (result == true && item.Name != dlg.Text)
            {
                if (_manipulator.Exists(CurrentPath + dlg.Text))
                {
                    var res = System.Windows.MessageBox.Show("Choose an other name!", "Filename allready exists!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _manipulator.Move(CurrentPath + item.Name, CurrentPath + dlg.Text);
                item.Name = dlg.Text;
                OnPropertyChanged("Items");
            }
        }

        private void OpenFolder(object parameter)
        {
            var item = parameter as ListItem;

            if (item == null)
                return;

            if (item.Name == "..")
            {
                if (CurrentPath == "/")
                    return;
                var tmp = CurrentPath.TrimEnd('/');
                CurrentPath = tmp.Substring(0, tmp.LastIndexOf("/")+1);
            } else { 
                CurrentPath += item.Name + "/";
            }
        }

        private void NewFolder(object parameter)
        {
            var newFolderName = "New Folder";
            
            if (_manipulator.Exists(CurrentPath + newFolderName))
            {
                var count = 1;
                while (_manipulator.Exists(CurrentPath + newFolderName + " " + count))
                    count++;
                newFolderName += " " + count;
            }
            _manipulator.CreateFolder(CurrentPath + newFolderName);
            
            Items.Add(new ListItem(CurrentPath, newFolderName, true));
        }


        private void NewVfs(object parameter)
        {
            // Create OpenFileDialog
            var dlg = new Microsoft.Win32.SaveFileDialog() { DefaultExt = ".vhs", Filter = "Virtual Filesystem (.vhs)|*.vhs" };
            
            // Display OpenFileDialog by calling ShowDialog method
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Close last vfs
                if (_manipulator != null)
                    _manipulator.Dispose();

                var sizeDlg = new InputViewModel("Size in MB", "10");
                result = sizeDlg.ShowDialog();

                if (result == true)
                {
                    // Open document
                    int size;
                    if (int.TryParse(sizeDlg.Text, out size))
                    {
                        var fileSystemData = new FileSystemOptions(dlg.FileName, size);
                        _manipulator = new FileSystemTextManipulator(fileSystemData);
                        CurrentPath = "/";
                    }
                }
            }
        }

        private void OpenVfs(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".vhs", Filter = "Virtual Filesystem (.vhs)|*.vhs" };
            var result = dlg.ShowDialog();

            if (result == true)
            {
                // Close last vfs
                if (_manipulator != null)
                    _manipulator.Dispose();

                var fileSystemData = new FileSystemOptions(dlg.FileName, 1000*1000*1000);
                _manipulator = new FileSystemTextManipulator(fileSystemData);
                CurrentPath = "/";
            }
        }

        private void ImportFile(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result == true)
            {
                if (_manipulator.Exists(CurrentPath + dlg.SafeFileName))
                {
                    var res = System.Windows.MessageBox.Show("Replace file?", "File allready exists!", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.No)
                        return;
                    _manipulator.Delete(CurrentPath + dlg.SafeFileName);
                    var listItem = Items.First(l => l.Name == dlg.SafeFileName);
                    Items.Remove(listItem);
                }

                _manipulator.Import(dlg.FileName, CurrentPath + dlg.SafeFileName);
                Items.Add(new ListItem(CurrentPath, dlg.SafeFileName, false));
            }
        }


    }
}
