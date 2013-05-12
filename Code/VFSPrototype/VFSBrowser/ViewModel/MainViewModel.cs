using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Practices.Unity;
using VFSBase.DiskServiceReference;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBrowser.Annotations;
using VFSBrowser.Helpers;
using DataFormats = System.Windows.DataFormats;
using MessageBox = System.Windows.MessageBox;

namespace VFSBrowser.ViewModel
{
    [UsedImplicitly]
    internal sealed class MainViewModel : AbstractViewModel, IMainViewModel
    {
        private IFileSystemTextManipulator _manipulator;
        private bool _copy;
        private readonly List<ListItem> _clipboard = new List<ListItem>();
        private readonly IUnityContainer _container;
        private UserDto _user;
        private readonly DiskServiceClient _diskService;

        private readonly ListItem _parent = new ListItem(null, "..", true);
        public ListItem Parent
        {
            get { return _parent; }
        }

        private DirectoryPath _currentPath;
        private long _versionInput;
        private long _latestVersion;
        private SynchronizationViewModel _synchronization;

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
                        // Loop until directory exists
                        while (!_manipulator.Exists(newValue.DisplayPath) || !_manipulator.IsDirectory(newValue.DisplayPath)) newValue.SwitchToParent();

                        Items.Clear();
                        if (!newValue.IsRoot) Items.Add(Parent);

                        foreach (var name in _manipulator.List(newValue.DisplayPath))
                        {
                            Items.Add(new ListItem(newValue.DisplayPath, name, _manipulator.IsDirectory(newValue.GetChild(name).DisplayPath)));
                        }
                        OnPropertyChanged("Items");

                    }
                    catch (Exception e)
                    {
                        UserMessage.Exception(e);
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

            OpenVfsCommand = new Command(OpenVfs, p => _manipulator == null);
            NewVfsCommand = new Command(NewVfs, p => _manipulator == null);
            CloseVfsCommand = new Command(CloseVfs, p => _manipulator != null);
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
            RollBackToVersionCommand = new Command(RollBackToVersion, p => (_manipulator != null));
            SwitchToLatestVersionCommand = new Command(SwitchToLatestVersion, p => (_manipulator != null));

            LoginCommand = new Command(Login, p => (_user == null));
            LogoutCommand = new Command(Logout, p => (_user != null));
            RegisterCommand = new Command(Register, p => (_user == null));
            LinkDiskCommand = new Command(LinkDisk, p => (_manipulator == null && _user != null));
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
        public Command RollBackToVersionCommand { get; private set; }
        public Command SwitchToLatestVersionCommand { get; private set; }

        public Command LoginCommand { get; private set; }
        public Command LogoutCommand { get; private set; }
        public Command RegisterCommand { get; private set; }
        public Command LinkDiskCommand { get; private set; }
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

        private void SwitchToLatestVersion(object parameter)
        {
            SwitchVersion(_manipulator.LatestVersion);
        }

        private void SwitchToVersion(object parameter)
        {
            SwitchVersion(VersionInput);
        }

        private void RollBackToVersion(object parameter)
        {
            var res = MessageBox.Show("You loose all changes if you do this. Are you shure you would like to continue?",
                            string.Format("Roll back to version {0}", VersionInput), MessageBoxButton.YesNo);
            if (res != MessageBoxResult.Yes) return;

            _manipulator.RollBackToVersion(VersionInput);
            SwitchVersion(VersionInput);
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                UserMessage.Exception(ex);
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

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Search(object parameter)
        {
            if (parameter != null)
                SearchOption.Keyword = parameter as string;

            Items.Clear();

            try
            {
                foreach (var i in _manipulator.Search(SearchOption.Keyword, SearchOption.Global ? DirectoryPath.Seperator : CurrentPath.Path, SearchOption.Recursive, SearchOption.CaseSensitive))
                {
                    var path = i.TrimEnd(DirectoryPath.Seperator.First());
                    var idx = path.LastIndexOf(DirectoryPath.Seperator, StringComparison.CurrentCulture) + 1;
                    var name = path.Substring(idx);
                    path = path.Substring(0, idx);
                    Items.Add(new ListItem(path, name, _manipulator.IsDirectory(i)));
                }
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
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
                UserMessage.Information("You are logged in", "Success");
            }
            catch (EndpointNotFoundException ex)
            {
                UserMessage.Error(string.Format("Unable to connect to server: {0}", ex.Message), "Login failed");
            }
            catch (FaultException<ServiceFault> ex)
            {
                UserMessage.Error(ex.Detail.Message, "Login failed");
            }
        }

        private void Logout(object parameter)
        {
            SwitchToOfflineMode(parameter);
            _user = null;
            UserMessage.Information("You are logged out", "Success");
        }

        private void Register(object parameter)
        {
            var vm = new RegisterDialogViewModel();

            if (vm.ShowDialog() != true) return;

            if (string.IsNullOrEmpty(vm.Password))
            {
                UserMessage.Error("Password must not be empty", "Registration failed");
                return;
            }

            try
            {
                // Note: we should encrypt the password with a salt...
                _user = _diskService.Register(vm.Login, HashHelper.GenerateHashCode(vm.Password));
                UserMessage.Information("You are registered", "Success");
            }
            catch (EndpointNotFoundException ex)
            {
                UserMessage.Error(string.Format("Unable to connect to server: {0}", ex.Message), "Registration failed");
            }
            catch (FaultException<ServiceFault> ex)
            {
                UserMessage.Error(ex.Detail.Message, "Registration failed");
            }
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void LinkDisk(object parameter)
        {
            try
            {
                if (_manipulator != null) return;
                if (_synchronization != null) return;
                if (_user == null) return;

                var viewModel = new DiskBrowserViewModel(_diskService, _user);
                if (!viewModel.ShowDialog() || viewModel.SelectedDisk == null) return;

                var selectedDisk = viewModel.SelectedDisk;
                var selectedLocation = viewModel.SelectedLocation;
                var diskOptions = _diskService.GetDiskOptions(_user, selectedDisk);

                _container.Resolve<IFileSystemTextManipulatorFactory>().LinkFileSystemTextManipulator(diskOptions, selectedLocation);
                OpenVfsWithPassword(selectedLocation);
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }
        }

        private void SynchronizationFinished()
        {
            ViewModelHelper.InvokeOnGuiThread(RefreshCurrentDirectory);
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SwitchToOnlineMode(object parameter)
        {
            try
            {
                if (_manipulator == null) return;
                if (_synchronization != null) return;
                if (_user == null) return;

                _synchronization = new SynchronizationViewModel(_manipulator, _user, SynchronizationFinished);

                UserMessage.Information("This disk will synchronize automatically now", "Switched to online mode");
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }
        }

        private void RefreshCurrentDirectory()
        {
            CurrentPath = CurrentPath;
        }

        private void SwitchToOfflineMode(object parameter)
        {
            if (_synchronization == null) return;

            _synchronization.StopSynchronization();
            _synchronization = null;
            UserMessage.Information("Synchronization stopped", "Switched to offline mode");
        }

        #endregion

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                UserMessage.Exception(ex);
            }
            UpdateVersion();
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                UserMessage.Exception(ex);
            }
            UpdateVersion();
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                UserMessage.Exception(ex);
            }
            UpdateVersion();
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Paste(object parameter)
        {
            try
            {
                foreach (var source in _clipboard)
                {
                    var sourcePath = source.Path + source.Name;
                    var destinationPath = CurrentPath.GetChild(source.Name).DisplayPath;

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

                        ViewModelHelper.RunAsyncAction(() => _manipulator.Copy(sourcePath, destinationPath, vm.Callbacks));
                        vm.ShowDialog();
                    }
                    else _manipulator.Move(sourcePath, destinationPath);

                    Items.Add(new ListItem(CurrentPath.DisplayPath, source.Name, _manipulator.IsDirectory(destinationPath)));
                }
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }
            UpdateVersion();
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                    ViewModelHelper.RunAsyncAction(() => _manipulator.Export(vfsExportPath, exportPath, vm.Callbacks));
                    vm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                }
                catch (Exception ex)
                {
                    UserMessage.Exception(ex);
                }
            }
            UpdateVersion();
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                    ViewModelHelper.RunAsyncAction(() => _manipulator.Export(item.Path + item.Name, tmpFile, vm.Callbacks));
                    vm.ShowDialog();

                    Process.Start("explorer", tmpFile);
                }
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void NewFolder(object parameter)
        {
            try
            {
                var newFolderName = "New Folder";
                if (_manipulator.Exists(CurrentPath.GetChild(newFolderName).DisplayPath))
                {
                    var count = 1;
                    while (_manipulator.Exists(CurrentPath.GetChild(newFolderName + " " + count).DisplayPath)) count++;
                    newFolderName += " " + count;
                }

                _manipulator.CreateFolder(CurrentPath.GetChild(newFolderName).DisplayPath);

                Items.Add(new ListItem(CurrentPath.DisplayPath, newFolderName, true));

            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }
            UpdateVersion();
        }

        private void CloseVfs(object parameter)
        {
            SwitchToOfflineMode(null);
            SwitchToLatestVersion(null);

            // Close last vfs
            _manipulator.FileSystemChanged -= FileSystemChanged;
            DisposeManipulator();
            _manipulator = null;
            Items.Clear();
            OnPropertyChanged("FileSystemName");
        }

        private void FileSystemChanged(object sender, FileSystemChangedEventArgs e)
        {
            ViewModelHelper.InvokeOnGuiThread(() => SwitchToLatestVersion(null));
            ViewModelHelper.InvokeOnGuiThread(RefreshCurrentDirectory);
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void NewVfs(object parameter)
        {
            var pathToVFS = ViewModelHelper.ChoosePlaceForNewVFSFile();
            if (pathToVFS == null) return;

            // Close last vfs
            DisposeManipulator();

            var vm = new NewVFSViewModel();
            if (vm.ShowDialog() != true) return;

            try
            {
                var fileSystemData = new FileSystemOptions(pathToVFS, vm.EncryptionType, vm.CompressionType);
                _manipulator = _container.Resolve<IFileSystemTextManipulatorFactory>().CreateFileSystemTextManipulator(fileSystemData, vm.Password);
                _manipulator.FileSystemChanged += FileSystemChanged;
                CurrentPath = new DirectoryPath();
                OnPropertyChanged("FileSystemName");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateVersion();
        }

        private void OpenVfs(object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { DefaultExt = ".vhs", Filter = "Virtual Filesystem (.vhs)|*.vhs" };
            if (dlg.ShowDialog() != true) return;
            var fileName = dlg.FileName;

            OpenVfsWithPassword(fileName);
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OpenVfsWithPassword(string fileName)
        {
            var passwordDialog = new PasswordDialogViewModel();
            if (passwordDialog.ShowDialog() != true) return;

            try
            {
                var manipulator = _container.Resolve<IFileSystemTextManipulatorFactory>().OpenFileSystemTextManipulator(fileName, passwordDialog.Password);

                // Close last vfs
                DisposeManipulator();

                if (_manipulator != null) _manipulator.FileSystemChanged -= FileSystemChanged;
                _manipulator = manipulator;
                _manipulator.FileSystemChanged += FileSystemChanged;

                CurrentPath = new DirectoryPath();
                OnPropertyChanged("FileSystemName");
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
            }
            UpdateVersion();
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                UserMessage.Exception(ex);
            }
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                UserMessage.Exception(ex);
            }
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                UserMessage.Exception(ex);
            }
        }

        // CA1031 does not apply here, because we want to catch any exception to display it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Import(string source, string name, bool isDirectory)
        {
            try
            {
                var virtualPath = CurrentPath.GetChild(name).DisplayPath;
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
                ViewModelHelper.RunAsyncAction(() => _manipulator.Import(source, virtualPath, vm.Callbacks));
                vm.ShowDialog();

                Items.Add(new ListItem(CurrentPath.DisplayPath, name, isDirectory));
            }
            catch (Exception ex)
            {
                UserMessage.Exception(ex);
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
}
