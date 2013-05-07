using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VFSBase.DiskServiceReference;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Synchronization
{
    class SynchronizationService
    {
        /// <summary>
        /// The synchronization interval specifies how long the pause in seconds between synchronization should be.
        /// </summary>
        private const int SynchronizationIntervalInSeconds = 3;
        private readonly IFileSystem _fileSystem;
        private readonly UserDto _user;
        private readonly SynchronizationCallbacks _callbacks;
        private static BackgroundWorker _backgroundWorker;
        private readonly DiskServiceClient _diskService;
        //private UserServiceClient _userService;
        private DiskDto _disk;

        public SynchronizationService(IFileSystem fileSystem, UserDto user, SynchronizationCallbacks callbacks)
        {
            _fileSystem = fileSystem;
            _user = user;
            _callbacks = callbacks;
            _diskService = new DiskServiceClient();
        }

        /*public static BackgroundWorker CreateService(IFileSystem fileSystem, User user, SynchronizationCallbacks callbacks)
        {
            var service = new SynchronizationService(fileSystem, user, callbacks);
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += service.DoWork;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerAsync();
            return backgroundWorker;
        }*/

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            return;
            var worker = sender as BackgroundWorker;
            if (worker == null) throw new ArgumentException("sender is not a background worker", "sender");

            while (!worker.CancellationPending)
            {
                var rwLock = _fileSystem.GetReadWriteLock();
                var version = _fileSystem.CurrentVersion;

                try
                {
                    rwLock.EnterWriteLock();
                    _fileSystem.SwitchToLatestVersion();

                    if (_disk == null) InitializeDisk();
                    Synchronize();
                }
                finally
                {
                    _fileSystem.SwitchToVersion(version);
                    if (rwLock.IsWriteLockHeld) rwLock.ExitWriteLock();
                }

                Thread.Sleep(SynchronizationIntervalInSeconds * 1000);
            }
        }

        private void Synchronize()
        {
            var state = _diskService.FetchSynchronizationState(_user, _disk);
            _callbacks.StateChanged(state);

            if (state == SynchronizationState.LocalChanges) SynchonizeLocalChanges();
            if (state == SynchronizationState.RemoteChanges) SynchronizeRemoteChanges();
        }

        private void SynchronizeRemoteChanges()
        {
            var remoteDisk = RemoteDisk();

            var untilBlockNr = remoteDisk.LastServerVersion;
            var localBlockNr = _fileSystem.Root.BlocksUsed;

            for (var currentBlockNr = localBlockNr + 1; currentBlockNr <= untilBlockNr; currentBlockNr++)
            {
                var data = _diskService.ReadBlock(_user, _disk.Id, currentBlockNr);
                _fileSystem.WriteBlock(currentBlockNr, data);
            }

            var options = _diskService.GetDiskOptions(_user, _disk);
            _fileSystem.WriteFileSystemOptions(options.SerializedFileSystemOptions);

            _fileSystem.FileSystemOptions.LocalVersion = _fileSystem.Root.Version;
            _fileSystem.FileSystemOptions.LastServerVersion = _fileSystem.Root.Version;
            _fileSystem.WriteConfig();

            using (var ms = new MemoryStream())
            {
                ms.Write(options.SerializedFileSystemOptions, 0, options.SerializedFileSystemOptions.Length);
                ms.Seek(0, SeekOrigin.Begin);

                IFormatter formatter = new BinaryFormatter();
                var fileSystemOptions = formatter.Deserialize(ms) as FileSystemOptions;
                if (fileSystemOptions == null) throw new VFSException("Invalid file");

                _fileSystem.Reload(fileSystemOptions);
            }
        }

        private DiskDto RemoteDisk()
        {
            return _diskService.Disks(_user).First(d => d.Id == _disk.Id);
        }

        private void SynchonizeLocalChanges()
        {
            var remoteDisk = RemoteDisk();

            _fileSystem.SwitchToVersion(remoteDisk.LocalVersion);
            var fromBlockNr = _fileSystem.Root.BlocksUsed;
            _fileSystem.SwitchToLatestVersion();
            var untilBlockNr = _fileSystem.Root.BlocksUsed;

            for (var currentBlockNr = fromBlockNr; currentBlockNr <= untilBlockNr; currentBlockNr++)
            {
                _diskService.WriteBlock(_user, _disk.Id, currentBlockNr, _fileSystem.ReadBlock(currentBlockNr));
            }

            _diskService.SetDiskOptions(_user, _disk, CalculateDiskOptions(_fileSystem.FileSystemOptions));

            _disk.LocalVersion = _fileSystem.Root.Version;
            _disk.LastServerVersion = _fileSystem.Root.Version;
            _diskService.UpdateDisk(_user, _disk);

            _fileSystem.FileSystemOptions.LocalVersion = _fileSystem.Root.Version;
            _fileSystem.FileSystemOptions.LastServerVersion = _fileSystem.Root.Version;
            _fileSystem.WriteConfig();
        }

        private void InitializeDisk()
        {
            if (_fileSystem.FileSystemOptions.Id == 0) CreateDisk();
            else LoadDisk();
        }

        private void LoadDisk()
        {
            var o = _fileSystem.FileSystemOptions;
            _disk = new DiskDto { LastServerVersion = o.LastServerVersion, LocalVersion = o.LocalVersion, Id = o.Id, UserId = _user.Id };
        }

        private void CreateDisk()
        {
            var o = _fileSystem.FileSystemOptions;

            var serverDisk = _diskService.CreateDisk(_user, CalculateDiskOptions(o));

            _fileSystem.MakeSynchronizedDisk(serverDisk.Id);
            LoadDisk();
        }

        private static DiskOptionsDto CalculateDiskOptions(IFileSystemOptions o)
        {
            byte[] serializedOptions;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, o);
                serializedOptions = ms.ToArray();
            }

            var diskOptions = new DiskOptionsDto
                                  {
                                      BlockSize = o.BlockSize,
                                      MasterBlockSize = o.MasterBlockSize,
                                      SerializedFileSystemOptions = serializedOptions
                                  };
            return diskOptions;
        }
    }

    internal class SynchronizationCallbacks
    {
        public SynchronizationCallbacks(Action<SynchronizationState> stateChanged)
        {
            StateChanged = stateChanged ?? (s => { });
        }

        public Action<SynchronizationState> StateChanged { get; private set; }
    }
}
