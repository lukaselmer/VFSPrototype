using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VFSBase.Callbacks;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    class ThreadSafeFileSystem : IFileSystem
    {
        private readonly FileSystem _fileSystem;
        private readonly ReaderWriterLockSlim _lock;

        internal ThreadSafeFileSystem(FileSystemOptions options)
        {
            _fileSystem = new FileSystem(options);
            _lock = _fileSystem.GetReadWriteLock();
        }

        public IEnumerable<IIndexNode> List(Folder folder)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _fileSystem.List(folder).ToList();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _fileSystem.Folders(folder).ToList();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<VFSFile> Files(Folder folder)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _fileSystem.Files(folder).ToList();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public bool Exists(Folder folder, string name)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _fileSystem.Exists(folder, name);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IIndexNode Find(Folder folder, string name)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _fileSystem.Find(folder, name);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public Folder CreateFolder(Folder parentFolder, string name)
        {
            _lock.EnterWriteLock();
            try
            {
                return _fileSystem.CreateFolder(parentFolder, name);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Import(string source, Folder destination, string name, CallbacksBase importCallbacks)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.Import(source, destination, name, importCallbacks);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Export(IIndexNode source, string destination, CallbacksBase exportCallbacks)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.Export(source, destination, exportCallbacks);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Copy(IIndexNode nodeToCopy, Folder destination, string nameOfCopiedElement, CallbacksBase copyCallbacks)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.Copy(nodeToCopy, destination, nameOfCopiedElement, copyCallbacks);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Delete(IIndexNode node)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.Delete(node);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Folder Root
        {
            get { return _fileSystem.Root; }
        }

        public FileSystemOptions FileSystemOptions
        {
            get { return _fileSystem.FileSystemOptions; }
        }

        public void TestEncryptionKey()
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                _fileSystem.TestEncryptionKey();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public long CurrentVersion
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    return _fileSystem.CurrentVersion;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        public long LatestVersion
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    return _fileSystem.LatestVersion;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        public void SwitchToLatestVersion()
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.SwitchToLatestVersion();
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
                _fileSystem.SwitchToVersion(version);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RollBackToVersion(long version)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.RollBackToVersion(version);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public ReaderWriterLockSlim GetReadWriteLock()
        {
            return _lock;
        }

        public bool IsSynchronizedDisk
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    return _fileSystem.IsSynchronizedDisk;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        public void MakeSynchronizedDisk(int id)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.MakeSynchronizedDisk(id);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public byte[] ReadBlock(long blockNumber)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                return _fileSystem.ReadBlock(blockNumber);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void WriteBlock(long blockNumber, byte[] block)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.WriteBlock(blockNumber, block);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void WriteFileSystemOptions(byte[] serializedFileSystemOptions)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.WriteFileSystemOptions(serializedFileSystemOptions);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void WriteConfig()
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.WriteConfig();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Reload(FileSystemOptions newOptions)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.Reload(newOptions);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public event EventHandler<FileSystemChangedEventArgs> FileSystemChanged
        {
            add { _fileSystem.FileSystemChanged += value; }
            remove { _fileSystem.FileSystemChanged -= value; }
        }

        public void OnFileSystemChanged(object sender, FileSystemChangedEventArgs e)
        {
            _lock.EnterWriteLock();
            try
            {
                _fileSystem.OnFileSystemChanged(sender, e);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            _fileSystem.Dispose();
        }
    }
}
