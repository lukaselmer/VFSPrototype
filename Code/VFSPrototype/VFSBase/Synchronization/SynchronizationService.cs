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
using VFSBase.Callbacks;
using VFSBase.DiskServiceReference;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Synchronization
{
    internal class SynchronizationService : ISynchronizationService
    {
        private readonly IFileSystem _fileSystem;
        private readonly UserDto _user;
        private readonly SynchronizationCallbacks _callbacks;
        private IDiskService _diskService;
        private DiskDto _disk;
        private readonly ReaderWriterLockSlim _lock;
        private IFileSystem fileSystem;
        private UserDto user;
        private SynchronizationCallbacks callbacks;
        private DiskServiceClient diskServiceClient;

        public SynchronizationService(IFileSystem fileSystem, UserDto user, SynchronizationCallbacks callbacks)
            : this(fileSystem, user, callbacks, new DiskServiceClient())
        { }

        protected SynchronizationService(IFileSystem fileSystem, UserDto user, SynchronizationCallbacks callbacks, IDiskService diskServiceClient)
        {
            _fileSystem = fileSystem;
            _user = user;
            _callbacks = callbacks;
            _lock = _fileSystem.GetReadWriteLock();
            _diskService = diskServiceClient;
        }

        public void Synchronize()
        {
            _lock.EnterWriteLock();
            try
            {
                var version = _fileSystem.CurrentVersion;
                try
                {
                    _fileSystem.SwitchToLatestVersion();

                    InitializeDisk();
                    DoSynchronize();
                }
                finally
                {
                    _fileSystem.SwitchToVersion(version);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            _callbacks.Finished();
        }

        private void DoSynchronize()
        {
            var state = FetchSynchronizationStateInternal();

            if (state == SynchronizationState.LocalChanges) SynchonizeLocalChanges();
            if (state == SynchronizationState.RemoteChanges) SynchronizeRemoteChanges();
            if (state == SynchronizationState.Conflicted)
                throw new VFSException("Synchronization is conflicted, please roll back until version is not conflicted anymore");
        }

        private SynchronizationState FetchSynchronizationStateInternal()
        {
            var state = _diskService.FetchSynchronizationState(_user, _disk);
            return state;
        }

        public SynchronizationState FetchSynchronizationState()
        {
            var rwLock = _fileSystem.GetReadWriteLock();

            try
            {
                rwLock.EnterWriteLock();

                InitializeDisk();

                var version = _fileSystem.CurrentVersion;
                try
                {
                    _fileSystem.SwitchToLatestVersion();
                    var state = FetchSynchronizationStateInternal();
                    return state;
                }
                finally
                {
                    _fileSystem.SwitchToVersion(version);
                }
            }
            finally
            {
                if (rwLock.IsWriteLockHeld) rwLock.ExitWriteLock();
            }
        }

        private void SynchronizeRemoteChanges()
        {
            var remoteDisk = RemoteDisk();

            var untilBlockNr = remoteDisk.NewestBlock;
            var localBlockNr = _fileSystem.Root.BlocksUsed;

            _callbacks.ProgressChanged(0, localBlockNr - untilBlockNr - 1);
            for (var currentBlockNr = localBlockNr + 1; currentBlockNr <= untilBlockNr; currentBlockNr++)
            {
                var data = _diskService.ReadBlock(_user, _disk.Id, currentBlockNr);
                _fileSystem.WriteBlock(currentBlockNr, data);
                _callbacks.ProgressChanged(currentBlockNr - localBlockNr - 1, untilBlockNr - localBlockNr - 1);
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
                var newFileSystemOptions = formatter.Deserialize(ms) as FileSystemOptions;
                if (newFileSystemOptions == null) throw new VFSException("Invalid file");

                _fileSystem.Reload(newFileSystemOptions);
            }

            _fileSystem.FileSystemOptions.LocalVersion = _fileSystem.Root.Version;
            _fileSystem.FileSystemOptions.LastServerVersion = _fileSystem.Root.Version;
            _fileSystem.WriteConfig();

            _fileSystem.OnFileSystemChanged(this, new FileSystemChangedEventArgs());
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

            _callbacks.ProgressChanged(0, fromBlockNr - untilBlockNr);
            for (var currentBlockNr = fromBlockNr; currentBlockNr <= untilBlockNr; currentBlockNr++)
            {
                _diskService.WriteBlock(_user, _disk.Id, currentBlockNr, _fileSystem.ReadBlock(currentBlockNr));
                _callbacks.ProgressChanged(currentBlockNr - fromBlockNr, untilBlockNr - fromBlockNr);
            }

            _diskService.SetDiskOptions(_user, _disk, SynchronizationHelper.CalculateDiskOptions(_fileSystem.FileSystemOptions));

            _disk.LocalVersion = _fileSystem.Root.Version;
            _disk.LastServerVersion = _fileSystem.Root.Version;
            _disk.NewestBlock = _fileSystem.Root.BlocksUsed;
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
            _disk = new DiskDto { LastServerVersion = o.LastServerVersion, LocalVersion = _fileSystem.LatestVersion, Id = o.Id, UserId = _user.Id };
        }

        private void CreateDisk()
        {
            var o = _fileSystem.FileSystemOptions;

            var serverDisk = _diskService.CreateDisk(_user, SynchronizationHelper.CalculateDiskOptions(o));

            _fileSystem.MakeSynchronizedDisk(serverDisk.Id);
            LoadDisk();
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

            if (_diskService != null)
            {
                var service = _diskService as DiskServiceClient;
                if (service != null) service.Close();

                _diskService = null;
            }
        }
    }
}
