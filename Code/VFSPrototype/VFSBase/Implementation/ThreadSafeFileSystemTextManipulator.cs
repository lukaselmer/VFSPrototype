using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VFSBase.Callbacks;
using VFSBase.DiskServiceReference;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    /// <summary>
    /// A thread safe file system text manipulator.
    /// Wraps the file system text manipulator and makes it thread safe.
    /// </summary>
    internal class ThreadSafeFileSystemTextManipulator : IFileSystemTextManipulator
    {
        private readonly FileSystemTextManipulator _manipulator;
        private readonly ReaderWriterLockSlim _lock;

        internal ThreadSafeFileSystemTextManipulator(IFileSystem fileSystem)
        {
            _lock = fileSystem.GetReadWriteLock();
            _manipulator = new FileSystemTextManipulator(fileSystem);
        }

        public IList<string> Search(string keyword, string folder, bool recursive, bool caseSensitive)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                return _manipulator.Search(keyword, folder, recursive, caseSensitive);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IList<string> Files(string path)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                return _manipulator.Files(path);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IList<string> List(string path)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                return _manipulator.List(path);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IList<string> Folders(string path)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                return _manipulator.Folders(path);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IList<string> Folders(string path, long version)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                return _manipulator.Folders(path, version);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public bool IsDirectory(string path)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                return _manipulator.IsDirectory(path);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void CreateFolder(string path)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.CreateFolder(path);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Copy(string source, string dest)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Copy(source, dest);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Copy(string source, string dest, CallbacksBase copyCallbacks)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Copy(source, dest, copyCallbacks);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Import(string source, string dest)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Import(source, dest);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Import(string source, string dest, CallbacksBase importCallbacks)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Import(source, dest, importCallbacks);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Export(string source, string dest)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Export(source, dest);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Export(string source, string dest, CallbacksBase exportCallbacks)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Export(source, dest, exportCallbacks);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Export(string source, string dest, CallbacksBase exportCallbacks, long version)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Export(source, dest, exportCallbacks, version);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Delete(string path)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Delete(path);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Move(string source, string dest)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.Move(source, dest);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Exists(string path)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _manipulator.Exists(path);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IFileSystemOptions FileSystemOptions
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    return _manipulator.FileSystemOptions;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        public long Version(string path)
        {
            _lock.EnterWriteLock();
            try
            {
                return _manipulator.Version(path);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void SwitchToVersion(long version)
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.SwitchToVersion(version);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void SwitchToLatestVersion()
        {
            _lock.EnterWriteLock();
            try
            {
                _manipulator.SwitchToLatestVersion();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<long> Versions(string path)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _manipulator.Versions(path);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public long LatestVersion
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    return _manipulator.LatestVersion;
                }
                finally
                {

                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        public ISynchronizationService GenerateSynchronizationService(UserDto user, SynchronizationCallbacks callbacks)
        {
            _lock.EnterWriteLock();
            try
            {
                return _manipulator.GenerateSynchronizationService(user, callbacks);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public event EventHandler<FileSystemChangedEventArgs> FileSystemChanged
        {
            add { _manipulator.FileSystemChanged += value; }
            remove { _manipulator.FileSystemChanged -= value; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            _manipulator.Dispose();
        }
    }
}
