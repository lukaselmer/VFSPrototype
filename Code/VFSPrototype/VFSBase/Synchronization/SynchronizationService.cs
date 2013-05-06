using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VFSBase.DiskServiceReference;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.UserServiceReference;
using User = VFSBase.DiskServiceReference.User;

namespace VFSBase.Synchronization
{
    class SynchronizationService
    {
        /// <summary>
        /// The synchronization interval specifies how long the pause in seconds between synchronization should be.
        /// </summary>
        private const int SynchronizationIntervalInSeconds = 3;
        private readonly IFileSystem _fileSystem;
        private readonly User _user;
        private readonly SynchronizationCallbacks _callbacks;
        private static BackgroundWorker _backgroundWorker;
        private DiskServiceClient _diskService;
        private UserServiceClient _userService;
        private Disk _disk;

        public SynchronizationService(IFileSystem fileSystem, User user, SynchronizationCallbacks callbacks)
        {
            _fileSystem = fileSystem;
            _user = user;
            _callbacks = callbacks;
            _diskService = new DiskServiceClient();
        }

        public static BackgroundWorker CreateService(IFileSystem fileSystem, User user, SynchronizationCallbacks callbacks)
        {
            var service = new SynchronizationService(fileSystem, user, callbacks);
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += service.DoWork;
            backgroundWorker.RunWorkerAsync();
            backgroundWorker.WorkerSupportsCancellation = true;
            return backgroundWorker;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker == null) throw new ArgumentException("sender is not a background worker", "sender");

            while (!worker.CancellationPending)
            {
                var rwLock = _fileSystem.GetReadWriteLock();

                try
                {
                    //_fileSystem.FileSystemOptions.
                    rwLock.EnterWriteLock();

                    if (_disk == null) InitializeDisk();
                    Synchronize();
                }
                finally
                {
                    if (rwLock.IsWriteLockHeld) rwLock.ExitWriteLock();
                }

                Thread.Sleep(SynchronizationIntervalInSeconds * 1000);
            }
        }

        private void Synchronize()
        {
            var state = _diskService.FetchSynchronizationState(_disk);
            _callbacks.StateChanged(state);

            if (state == SynchronizationState.LocalChanges) SynchonizeLocalChanges();
            if (state == SynchronizationState.RemoteChanges) SynchronizeRemoteChanges();
        }

        private void SynchronizeRemoteChanges()
        {
            var remoteDisk = RemoteDisk();
            //TODO: get blocks from the server, store them in the _fileSystem, adjust root and _blockAllocator
        }

        private Disk RemoteDisk()
        {
            return _diskService.Disks(_user).First(d => d.Uuid == _disk.Uuid);
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
                _diskService.WriteBlock(_disk.Uuid, currentBlockNr, _fileSystem.ReadBlock(currentBlockNr));
            }
        }

        private void SynchronizeNextLocalVersion()
        {
        }

        private void InitializeDisk()
        {
            if (_fileSystem.FileSystemOptions.Uuid == null) CreateDisk();
            else LoadDisk();
        }

        private void LoadDisk()
        {
            var o = _fileSystem.FileSystemOptions;
            _disk = new Disk { LastServerVersion = o.LastServerVersion, LocalVersion = o.LocalVersion, Uuid = o.Uuid, User = _user };
        }

        private void CreateDisk()
        {
            var o = _fileSystem.FileSystemOptions;

            byte[] serializedOptions;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, o);
                serializedOptions = ms.ToArray();
            }

            var diskOptions = new DiskOptions { BlockSize = o.BlockSize, MasterBlockSize = o.MasterBlockSize, SerializedFileSystemOptions = serializedOptions };
            var serverDisk = _diskService.CreateDisk(_user, diskOptions);

            _fileSystem.MakeSynchronizedDisk(serverDisk.Uuid);
            LoadDisk();
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
