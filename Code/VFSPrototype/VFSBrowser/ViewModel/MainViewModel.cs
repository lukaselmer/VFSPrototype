using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using VFSBase.DiskServiceReference;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Synchronization;
using VFSBrowser.Annotations;
using VFSBrowser.Helpers;
using DataFormats = System.Windows.DataFormats;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace VFSBrowser.ViewModel
{
    [UsedImplicitly]
    internal sealed class MainViewModel : AbstractViewModel, IMainViewModel
    {
        private IFileSystemTextManipulator _manipulator;
        private bool _copy;
        private readonly List<ListItem> _clipboard = new List<ListItem>();
        private readonly IUnityContainer _container;
        private ISynchronizationService _synchronization;
        private UserDto _user;
        private DiskServiceClient _diskService;

        private readonly ListItem _parent = new ListItem(null, "..", true);
        public ListItem Parent
        {
            get { return _parent; }
        }

        private DirectoryPath _currentPath;
        private long _versionInput;
        private long _latestVersion;

        public DirectoryPath CurrentPath
        {
            get { return _currentPath; }
            set
            {
                var newValue = value;
                if (_manipulator != null)
                {
                    try
                    {
                        while (!_manipulator.Exists(newValue.ToString()) || !_manipulator.IsDirectory(newValue.ToString())) newValue.SwitchToParent();

                        Items.Clear();
                        if (!newValue.IsRoot) Items.Add(Parent);

                        foreach (var name in _manipulator.List(newValue.ToString()))
                        {
                            Items.Add(new ListItem(newValue.ToString(), name, _manipulator.IsDirectory(newValue.GetChild(name).ToString())));
                        }
                        OnPropertyChanged("Items");

                    }
                    catch (Exception e)
                    {
                        DisplayException(e);
                    }
                }
                _currentPath = newValue;
                OnPropertyChanged("CurrentPath");
            }
        }

        public long VersionInput
        {
            get { return _versionInput; }
            set
            {
                _versionInput = value;
                OnPropertyChanged("VersionInput");
            }
        }

        public long LatestVersion
        {
            get { return _latestVersion; }
            set
            {
                _latestVersion = value;
                OnPropertyChanged("LatestVersion");
            }
        }

        public string FileSystemName
        {
            get { return _manipulator != null ? ("VFS Browser - " + _manipulator.FileSystemOptions.Location) : "VFS Browser"; }
        }

        public SearchOption SearchOption { get; set; }

        public ObservableCollection<ListItem> Items { get; set; }

        public MainViewModel(IUnityContainer container)
        {
            _container = container;
            Items = new ObservableCollection<ListItem>();
            SearchOption = new SearchOption { CaseSensitive = false, Recursive = true, Keyword = "", Global = false };

            OpenVfsCommand = new Command(OpenVfs, null);
            NewVfsCommand = new Command(NewVfs, null);
            CloseVfsCommand = new Command(CloseVfs, null);
            NewFolderCommand = new Command(NewFolder, p => (_manipulator != null));
            OpenCommand = new Command(Open, p => (_manipulator != null && p != null));
            RenameCommand = new Command(Rename, IsItemSelected);
            ImportFileCommand = new Command(ImportFile, p => (_manipulator != null));
            ImportFolderCommand = new Command(ImportFolder, p => (_manipulator != null));
            ExportCommand = new Command(Export, IsItemSelected);
            DeleteCommand = new Command(Delete, IsItemSelected);
            MoveCommand = new Command(Move, IsItemSelected);
            CopyCommand = new Command(Copy, IsItemSelected);
            PasteCommand = new Command(Paste, p => (_manipulator != null && _clipboard.Count > 0));
            SearchCommand = new Command(Search, p => (_manipulator != null));
            CancelSearchCommand = new Command(CancelSearch, p => (_manipulator != null));
            DiskInfoCommand = new Command(DiskInfo, p => (_manipulator != null));
            SwitchToVersionCommand = new Command(SwitchToVersion, p => (_manipulator != null));
            SwitchToLatestVersionCommand = new Command(SwitchToLatestVersion, p => (_manipulator != null));

            LoginCommand = new Command(Login, p => (_user == null));
            LogoutCommand = new Command(Logout, p => (_user != null));
            RegisterCommand = new Command(Register, p => (_user == null));
            SwitchToOnlineModeCommand = new Command(SwitchToOnlineMode, p => (_manipulator != null && _user != null && _synchronization == null));
            SwitchToOfflineModeCommand = new Command(SwitchToOfflineMode, p => (_manipulator != null && _user != null && _synchronization != null));

            DropCommand = new Command(Drop, null);

            _diskService = new DiskServiceClient();
        }


        public Command DropCommand { get; private set; }

        public Command OpenVfsCommand { get; private set; }
        public Command NewVfsCommand { get; private set; }
        public Command CloseVfsCommand { get; private set; }
        public Command NewFolderCommand { get; private set; }
        public Command OpenCommand { get; private set; }
        public Command RenameCommand { get; private set; }
        public Command ImportFileCommand { get; private set; }
        public Command ImportFolderCommand { get; private set; }
        public Command ExportCommand { get; private set; }
        public Command CopyCommand { get; private set; }
        public Command MoveCommand { get; private set; }
        public Command PasteCommand { get; private set; }
        public Command DeleteCommand { get; private set; }
        public Command SearchCommand { get; private set; }
        public Command CancelSearchCommand { get; private set; }
        public Command DiskInfoCommand { get; private set; }
        public Command SwitchToVersionCommand { get; private set; }
        public Command SwitchToLatestVersionCommand { get; private set; }

        public Command LoginCommand { get; private set; }
        public Command LogoutCommand { get; private set; }
        public Command RegisterCommand { get; private set; }
        public Command SwitchToOnlineModeCommand { get; private set; }
        public Command SwitchToOfflineModeCommand { get; private set; }

        private bool IsItemSelected(object parameter)
        {
            if (_manipulator == null) return false;
            if (parameter == null) return false;

            var items = parameter as ObservableCollection<object>;
            if (items != null)
            {
                if (items.Count <= 0) return false;
                var listItem = items[0] as ListItem;
                return listItem != null && (items.Count > 0 && (items.Count != 1 || listItem.Name != ".."));
            }

            var item = parameter as ListItem;
            return item != null && item.Name != "..";
        }

        //private List<ListItem> SearchItems(string folder)
        //{
        //    var items = new List<ListItem>();
        //    foreach (var item in _manipulator.List(folder))
        //    {
        //        if (SearchOption.Recursive && _manipulator.IsDirectory(folder + item))
        //        {
        //            items.AddRange(SearchItems(folder + item + DirectoryPath.Seperator));
        //        }

        //        var name = item;
        //        var searchName = SearchOption.Keyword;
        //        if (SearchOption.CaseSensitive == false)
        //        {
        //            name = name.ToLower();
        //            searchName = searchName.ToLower();
        //        }

        //        if (name.Contains(searchName))
        //        {
        //            items.Add(new ListItem(folder, item, _manipulator.IsDirectory(folder + item)));
        //        }
        //    }
        //    return items;
        //}

        private void SwitchToLatestVersion(object parameter)
        {
            SwitchVersion(LatestVersion);
        }

        private void SwitchToVersion(object parameter)
        {
            SwitchVersion(VersionInput);
        }

        private void SwitchVersion(long version)
        {
            try
            {
                _manipulator.SwitchToVersion(version);
                RefreshCurrentDirectory();
                OnPropertyChanged("FileSystemName");
                UpdateVersion();
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void UpdateVersion()
        {
            if (_manipulator == null) return;
            LatestVersion = _manipulator.LatestVersion;
            var version = _manipulator.Version(DirectoryPath.Seperator);
            VersionInput = version;
        }

        private void CancelSearch(object parameter)
        {
            SearchOption.Keyword = "";
            OnPropertyChanged("SearchOption");
            RefreshCurrentDirectory();
        }

        private void Search(object parameter)
        {
            if (parameter != null)
                SearchOption.Keyword = parameter as string;

            Items.Clear();
            //SearchItems(CurrentPath).ForEach(i => Items.Add(i));

            try
            {
                foreach (var i in _manipulator.Search(SearchOption.Keyword, SearchOption.Global ? DirectoryPath.Seperator : CurrentPath.ToString(), SearchOption.Recursive, SearchOption.CaseSensitive))
                {
                    var idx = i.LastIndexOf(DirectoryPath.Seperator, StringComparison.CurrentCulture) + 1;
                    var name = i.Substring(idx);
                    var path = i.Substring(0, idx);
                    Items.Add(new ListItem(path, name, _manipulator.IsDirectory(i)));
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void DiskInfo(object parameter)
        {
            var sizeDlg = new DiskInfoViewModel(_manipulator);
            sizeDlg.ShowDialog();
        }

        #region Synchronization / Registration / Login

        private void Login(object parameter)
        {
            var vm = new LoginDialogViewModel();

            if (vm.ShowDialog() != true) return;

            try
            {
                _user = _diskService.Login(vm.Login, HashHelper.GenerateHashCode(vm.Password));
            }
            catch (EndpointNotFoundException ex)
            {
                MessageBox.Show(string.Format("Unable to connect to server: {0}", ex.Message), "Login failed");
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Login failed");
            }
        }

        private void Logout(object parameter)
        {
            SwitchToOfflineMode(parameter);
            _user = null;
        }

        private void Register(object parameter)
        {
            var vm = new RegisterDialogViewModel();

            if (vm.ShowDialog() != true) return;

            if (string.IsNullOrEmpty(vm.Password))
            {
                MessageBox.Show("Password must not be empty", "Registration failed");
                return;
            }

            try
            {
                // Note: we should encrypt the password with a salt...
                _user = _diskService.Register(vm.Login, HashHelper.GenerateHashCode(vm.Password));
            }
            catch (EndpointNotFoundException ex)
            {
                MessageBox.Show(string.Format("Unable to connect to server: {0}", ex.Message), "Registration failed");
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Registration failed");
            }
        }

        private void SwitchToOnlineMode(object parameter)
        {
            try
            {
                if (_manipulator == null) return;
                if (_synchronization != null) return;
                if (_user == null) return;
                _synchronization = _manipulator.GenerateSynchronizationService(_user, new SynchronizationCallbacks(SynchronizationStateChanged));
                _synchronization.Synchronize();
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void SynchronizationStateChanged(SynchronizationState state)
        {
            RefreshCurrentDirectory();
        }

        private void RefreshCurrentDirectory()
        {
            CurrentPath = CurrentPath;
        }

        private void SwitchToOfflineMode(object parameter)
        {
            _synchronization = null;
        }

        #endregion


        private void Delete(object parameter)
        {
            var deleteItems = parameter as ObservableCollection<object>;

            if (deleteItems == null) return;

            try
            {
                var del = new List<ListItem>();
                foreach (ListItem item in deleteItems)
                {
                    _manipulator.Delete(item.Path + item.Name);
                    del.Add(item);
                }

                del.ForEach(i => Items.Remove(i));
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            UpdateVersion();
        }

        private void DisplayException(Exception exception)
        {
            MessageBox.Show(exception.Message, "An error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Copy(object parameter)
        {
            var items = parameter as ObservableCollection<object>;

            if (items == null) return;

            try
            {
                _clipboard.Clear();
                _copy = true;
                foreach (ListItem item in items) _clipboard.Add(item);
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            UpdateVersion();
        }

        private void Move(object parameter)
        {
            var items = parameter as ObservableCollection<object>;

            if (items == null) return;

            try
            {
                _clipboard.Clear();
                _copy = false;
                foreach (ListItem item in items)
                    _clipboard.Add(item);

            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            UpdateVersion();
        }

        private void Paste(object parameter)
        {
            try
            {
                foreach (var source in _clipboard)
                {
                    var sourcePath = source.Path + source.Name;
                    var destinationPath = CurrentPath.GetChild(source.Name).ToString();

                    if (!_manipulator.Exists(sourcePath))
                    {
                        MessageBox.Show("File or folder to copy does not exist", "Path not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }


                    if (_manipulator.Exists(destinationPath))
                    {
                        var result = MessageBox.Show("Replace file?", "File already exists!", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.No) continue;

                        _manipulator.Delete(destinationPath);
                        var listItem = Items.First(l => l.Name == source.Name);
                        Items.Remove(listItem);
                    }

                    if (_copy)
                    {
                        var vm = new OperationProgressViewModel();

                        RunAsyncAction(() => _manipulator.Copy(sourcePath, destinationPath, vm.Callbacks));
                        vm.ShowDialog();
                    }
                    else _manipulator.Move(sourcePath, destinationPath);

                    Items.Add(new ListItem(CurrentPath.ToString(), source.Name, _manipulator.IsDirectory(destinationPath)));
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            UpdateVersion();
        }

        private void RunAsyncAction(Action action)
        {
            Action<Task> handleException = task => { if (task.Exception != null) DisplayException(task.Exception.InnerException); };
            Task.Run(action).ContinueWith(handleException, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Export(object parameter)
        {
            var items = parameter as ObservableCollection<object>;

            if (items == null) return;

            var dlg = new FolderBrowserDialog { ShowNewFolderButton = true };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                foreach (ListItem item in items)
                {
                    var exportPath = Path.Combine(dlg.SelectedPath, item.Name);
                    var vfsExportPath = item.Path + item.Name;

                    if (File.Exists(exportPath) || Directory.Exists(exportPath))
                    {
                        var messageBoxText = string.Format("Replace {0}? (Current item will be deleted if you choose yes!)", Path.GetFullPath(exportPath));
                        var result = MessageBox.Show(messageBoxText, "Object already exists!", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.No) continue;

                        if (Directory.Exists(exportPath))
                        {
                            try
                            {
                                Directory.Delete(exportPath, true);
                            }
                            catch (IOException)
                            {
                                // Give the OS time to release potential resources
                                Thread.Sleep(0);
                                Directory.Delete(exportPath, true);
                            }
                        }
                        if (File.Exists(exportPath)) File.Delete(exportPath);
                    }

                    var vm = new OperationProgressViewModel();
                    RunAsyncAction(() => _manipulator.Export(vfsExportPath, exportPath, vm.Callbacks));
                    vm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
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
                    if (_manipulator.Exists(item.Path + dlg.Text))
                    {
                        MessageBox.Show("Please choose an other name.", "Filename already exists!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _manipulator.Move(item.Path + item.Name, item.Path + dlg.Text);
                    RefreshCurrentDirectory();
                    //item.Name = dlg.Text;
                    //OnPropertyChanged("Items");

                }
                catch (Exception ex)
                {
                    DisplayException(ex);
                }
            }
            UpdateVersion();
        }

        private void Open(object parameter)
        {
            var item = parameter as ListItem;

            if (item == null)
                return;
            try
            {
                if (item.IsDirectory)
                {
                    if (item.Name == "..")
                    {
                        if (CurrentPath.IsRoot) return;

                        CurrentPath.SwitchToParent();
                        RefreshCurrentDirectory();
                    }
                    else
                    {
                        CurrentPath = new DirectoryPath(item.Path, item.Name);
                    }
                }
                else
                {
                    var tmpFile = Path.GetTempPath() + item.Name;
                    if (File.Exists(tmpFile)) File.Delete(tmpFile);

                    var vm = new OperationProgressViewModel();
                    RunAsyncAction(() => _manipulator.Export(item.Path + item.Name, tmpFile, vm.Callbacks));
                    vm.ShowDialog();

                    Process.Start("explorer", tmpFile);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void NewFolder(object parameter)
        {
            try
            {
                var newFolderName = "New Folder";
                if (_manipulator.Exists(CurrentPath.GetChild(newFolderName).ToString()))
                {
                    var count = 1;
                    while (_manipulator.Exists(CurrentPath.GetChild(newFolderName + " " + count).ToString())) count++;
                    newFolderName += " " + count;
                }

                _manipulator.CreateFolder(CurrentPath.GetChild(newFolderName).ToString());

                Items.Add(new ListItem(CurrentPath.ToString(), newFolderName, true));

            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            UpdateVersion();
        }

        private void CloseVfs(object parameter)
        {
            // Close last vfs
            DisposeManipulator();
            _manipulator = null;
            Items.Clear();
            OnPropertyChanged("FileSystemName");
        }

        private void NewVfs(object parameter)
        {
            var pathToVFS = ChooseNewVFSFile();
            if (pathToVFS == null) return;

            // Close last vfs
            DisposeManipulator();

            var vm = new NewVFSViewModel();
            if (vm.ShowDialog() != true) return;

            try
            {
                var fileSystemData = new FileSystemOptions(pathToVFS, vm.MaximumSize, vm.EncryptionType, vm.CompressionType);
                _manipulator = _container.Resolve<IFileSystemTextManipulatorFactory>().CreateFileSystemTextManipulator(fileSystemData, vm.Password);
                CurrentPath = new DirectoryPath();
                OnPropertyChanged("FileSystemName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateVersion();
        }

        private static string ChooseNewVFSFile()
        {
            // Create OpenFileDialog
            var dlg = new SaveFileDialog { DefaultExt = ".vhs", Filter = "Virtual Filesystem (.vhs)|*.vhs" };

            // Display OpenFileDialog by calling ShowDialog method
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            return result != true ? null : dlg.FileName;
        }

        private void OpenVfs(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { DefaultExt = ".vhs", Filter = "Virtual Filesystem (.vhs)|*.vhs" };
            if (dlg.ShowDialog() != true) return;

            var passwordDialog = new PasswordDialogViewModel();
            if (passwordDialog.ShowDialog() != true) return;

            try
            {
                var manipulator = _container.Resolve<IFileSystemTextManipulatorFactory>().OpenFileSystemTextManipulator(dlg.FileName, passwordDialog.Password);

                // Close last vfs
                DisposeManipulator();

                _manipulator = manipulator;
                CurrentPath = new DirectoryPath();
                OnPropertyChanged("FileSystemName");
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            UpdateVersion();
        }

        private void Drop(object inObject)
        {
            var dragArgs = inObject as System.Windows.DragEventArgs;
            if (null == dragArgs) return;

            var data = dragArgs.Data;

            if (!data.GetDataPresent(DataFormats.FileDrop)) return;
            try
            {
                var files = (string[])data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    Import(file, name, Directory.Exists(file));
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void ImportFile(object parameter)
        {
            var dlg = new OpenFileDialog { AutoUpgradeEnabled = true, CheckFileExists = true, CheckPathExists = true, Multiselect = true };
            var result = dlg.ShowDialog();

            if (result != DialogResult.OK) return;

            try
            {
                for (var i = 0; i < dlg.FileNames.Length; i++)
                {
                    Import(dlg.FileNames[i], dlg.SafeFileNames[i], false);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void ImportFolder(object parameter)
        {
            var dlg = new FolderBrowserDialog();
            var result = dlg.ShowDialog();

            if (result != DialogResult.OK) return;

            try
            {
                Import(dlg.SelectedPath, Path.GetFileName(dlg.SelectedPath), true);
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void Import(string source, string name, bool isDirectory)
        {
            try
            {
                var virtualPath = CurrentPath.GetChild(name).ToString();
                if (_manipulator.Exists(virtualPath))
                {
                    var messageBoxText = string.Format("Replace {0}?", isDirectory ? "folder" : "file");
                    var caption = string.Format("{0} already exists!", isDirectory ? "Folder" : "File");
                    var res = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.No) return;

                    _manipulator.Delete(virtualPath);
                    var listItem = Items.First(l => l.Name == name);
                    Items.Remove(listItem);
                }

                var vm = new OperationProgressViewModel();
                RunAsyncAction(() => _manipulator.Import(source, virtualPath, vm.Callbacks));
                vm.ShowDialog();

                Items.Add(new ListItem(CurrentPath.ToString(), name, isDirectory));
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            UpdateVersion();
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

    public interface IMainViewModel : IDisposable
    {
    }
}
