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
using VFSBase.Interfaces;
using MessageBox = System.Windows.MessageBox;

namespace VFSBrowser.ViewModel
{
    sealed class MainViewModel : AbstractViewModel, IDisposable
    {
        private IFileSystemTextManipulator _manipulator;

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
                try {
                    Items.Clear();
                    Items.Add(Parent);
                    foreach (var name in _manipulator.List(value))
                    {
                        Items.Add(new ListItem(value, name, _manipulator.IsDirectory(value + name)));
                    }

                } catch (Exception e) {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                _currentPath = value;
                OnPropertyChanged("CurrentPath");
                
            }
        }

        public SearchOption SearchOption { get; set; }

        public ObservableCollection<ListItem> Items { get; set; }

        public MainViewModel()
        {
            Items = new ObservableCollection<ListItem>();
            SearchOption = new SearchOption { CaseSensitive = false, Recursive = true, SearchText = ""};
           
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
            SearchCommand = new Command(Search, p => (_manipulator != null));
            DiskInfoCommand = new Command(DiskInfo, p => (_manipulator != null));
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
        public Command SearchCommand { get; private set; }
        public Command DiskInfoCommand { get; private set; }


        private List<ListItem> SearchItems(string folder)
        {
            var items = new List<ListItem>();
            foreach (var item in _manipulator.List(folder))
            {
                if (SearchOption.Recursive && _manipulator.IsDirectory(folder + item))
                {
                    items.AddRange(SearchItems(folder + item + "/"));
                }

                var name = item;
                var searchName = SearchOption.SearchText;
                if (SearchOption.CaseSensitive == false)
                {
                    name = name.ToLower();
                    searchName = searchName.ToLower();
                }

                if (name.Contains(searchName))
                {
                    items.Add(new ListItem(folder, item, _manipulator.IsDirectory(folder + item)));
                }
            }
            return items;
        } 

        private void Search(object parameter)
        {
            if (parameter != null)
                SearchOption.SearchText = parameter as string;

            Items.Clear();
            SearchItems(CurrentPath).ForEach(i => Items.Add(i));
        }

        private void DiskInfo(object parameter)
        {

            var sizeDlg = new DiskInfoViewModel(_manipulator);
            sizeDlg.ShowDialog();
        }

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

            try {
                _clipboard.Clear();
                _copy = true;
                foreach (ListItem item in items)
                    _clipboard.Add(item);

            } catch (Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Move(object parameter)
        {
            var items = parameter as ObservableCollection<object>;

            if (items == null)
                return;

            try {
                _clipboard.Clear();
                _copy = false;
                foreach (ListItem item in items)
                    _clipboard.Add(item);

            } catch (Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Paste(object parameter)
        {
            try
            {
                foreach (var source in _clipboard)
                {
                    if (_manipulator.Exists(CurrentPath + source.Name))
                    {
                        var result = MessageBox.Show("Replace file?", "File allready exists!", MessageBoxButton.YesNo,
                                                                    MessageBoxImage.Information);
                        if (result == MessageBoxResult.No)
                            continue;
                        _manipulator.Delete(CurrentPath + source.Name);
                        var listItem = Items.First(l => l.Name == source.Name);
                        Items.Remove(listItem);
                    }

                    if (_copy)
                        _manipulator.Copy(source.Path + source.Name, CurrentPath + source.Name, new CopyCallbacks());
                    else
                        _manipulator.Move(source.Path + source.Name, CurrentPath + source.Name);

                    Items.Add(new ListItem(CurrentPath, source.Name, _manipulator.IsDirectory(CurrentPath + source.Name)));
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                try {
                    foreach (ListItem item in items)
                        _manipulator.Export(CurrentPath + item.Name, dlg.SelectedPath + "\\" + item.Name, new ExportCallbacks());

                } catch (Exception e) {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                try
                {
                    if (_manipulator.Exists(CurrentPath + dlg.Text))
                    {
                        var res = MessageBox.Show("Choose an other name!", "Filename allready exists!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _manipulator.Move(CurrentPath + item.Name, CurrentPath + dlg.Text);
                    item.Name = dlg.Text;
                    OnPropertyChanged("Items");

                } catch (Exception e) {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                CurrentPath = item.Path + item.Name + "/";
            }
        }

        private void NewFolder(object parameter)
        {
            try
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

            } catch (Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void NewVfs(object parameter)
        {
            // Create OpenFileDialog
            var dlg = new Microsoft.Win32.SaveFileDialog { DefaultExt = ".vhs", Filter = "Virtual Filesystem (.vhs)|*.vhs" };
            
            // Display OpenFileDialog by calling ShowDialog method
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Close last vfs
                DisposeManipulator();

                var sizeDlg = new InputViewModel("Size in MB", "10");
                result = sizeDlg.ShowDialog();

                if (result == true)
                {
                    // Open document
                    int size;
                    if (int.TryParse(sizeDlg.Text, out size))
                    {
                        try {
                            var fileSystemData = new FileSystemOptions(dlg.FileName, size);
                            _manipulator = new FileSystemTextManipulator(fileSystemData);
                            CurrentPath = "/";

                        } catch (Exception e) {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void OpenVfs(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { DefaultExt = ".vhs", Filter = "Virtual Filesystem (.vhs)|*.vhs" };
            var result = dlg.ShowDialog();

            if (result == true)
            {
                // Close last vfs
                DisposeManipulator();

                try {
                    var fileSystemData = new FileSystemOptions(dlg.FileName, 1000*1000*1000);
                    _manipulator = new FileSystemTextManipulator(fileSystemData);
                    CurrentPath = "/";

                } catch (Exception e) {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportFile(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result == true)
            {
                try {
                    if (_manipulator.Exists(CurrentPath + dlg.SafeFileName))
                    {
                        var res = System.Windows.MessageBox.Show("Replace file?", "File allready exists!", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (res == MessageBoxResult.No)
                            return;
                        _manipulator.Delete(CurrentPath + dlg.SafeFileName);
                        var listItem = Items.First(l => l.Name == dlg.SafeFileName);
                        Items.Remove(listItem);
                    }

                    _manipulator.Import(dlg.FileName, CurrentPath + dlg.SafeFileName, new ImportCallbacks());
                    Items.Add(new ListItem(CurrentPath, dlg.SafeFileName, false));

                } catch (Exception e) {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DisposeManipulator()
        {
            if (_manipulator != null)
                _manipulator.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource.

            if (!disposing) return;

            // free managed resources

            DisposeManipulator();
        }
    }
}
