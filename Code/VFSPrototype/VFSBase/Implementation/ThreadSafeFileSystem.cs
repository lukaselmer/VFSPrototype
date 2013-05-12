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
        private FileSystem o;
        private ReaderWriterLockSlim l;

        internal ThreadSafeFileSystem(FileSystemOptions options)
        {
            o = new FileSystem(options);
            l = o.GetReadWriteLock();
        }

        public IEnumerable<IIndexNode> List(Folder folder)
        {
            l.EnterUpgradeableReadLock();
            try
            {
                return o.List(folder).ToList();
            }
            finally
            {
                l.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<Folder> Folders(Folder folder)
        {
            l.EnterUpgradeableReadLock();
            try
            {
                return o.Folders(folder).ToList();
            }
            finally
            {
                l.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<VFSFile> Files(Folder folder)
        {
            l.EnterUpgradeableReadLock();
            try
            {
                return o.Files(folder).ToList();
            }
            finally
            {
                l.ExitUpgradeableReadLock();
            }
        }

        public bool Exists(Folder folder, string name)
        {
            l.EnterUpgradeableReadLock();
            try
            {
                return o.Exists(folder, name);
            }
            finally
            {
                l.ExitUpgradeableReadLock();
            }
        }

        public IIndexNode Find(Folder folder, string name)
        {
            l.EnterUpgradeableReadLock();
            try
            {
                return o.Find(folder, name);
            }
            finally
            {
                l.ExitUpgradeableReadLock();
            }
        }

        public Folder CreateFolder(Folder parentFolder, string name)
        {
            l.EnterWriteLock();
            try
            {
                return o.CreateFolder(parentFolder, name);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void Import(string source, Folder destination, string name, CallbacksBase importCallbacks)
        {
            l.EnterWriteLock();
            try
            {
                o.Import(source, destination, name, importCallbacks);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void Export(IIndexNode source, string destination, CallbacksBase exportCallbacks)
        {
            l.EnterWriteLock();
            try
            {
                o.Export(source, destination, exportCallbacks);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void Copy(IIndexNode nodeToCopy, Folder destination, string nameOfCopiedElement, CallbacksBase copyCallbacks)
        {
            l.EnterWriteLock();
            try
            {
                o.Copy(nodeToCopy, destination, nameOfCopiedElement, copyCallbacks);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void Delete(IIndexNode node)
        {
            l.EnterWriteLock();
            try
            {
                o.Delete(node);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public Folder Root
        {
            get { return o.Root; }
        }

        public FileSystemOptions FileSystemOptions
        {
            get { return o.FileSystemOptions; }
        }

        public void TestEncryptionKey()
        {
            l.EnterUpgradeableReadLock();
            try
            {
                o.TestEncryptionKey();
            }
            finally
            {
                l.ExitUpgradeableReadLock();
            }
        }

        public long CurrentVersion
        {
            get
            {
                l.EnterUpgradeableReadLock();
                try
                {
                    return o.CurrentVersion;
                }
                finally
                {
                    l.ExitUpgradeableReadLock();
                }
            }
        }

        public long LatestVersion
        {
            get
            {
                l.EnterUpgradeableReadLock();
                try
                {
                    return o.LatestVersion;
                }
                finally
                {
                    l.ExitUpgradeableReadLock();
                }
            }
        }

        public void SwitchToLatestVersion()
        {
            l.EnterWriteLock();
            try
            {
                o.SwitchToLatestVersion();
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void SwitchToVersion(long version)
        {
            l.EnterWriteLock();
            try
            {
                o.SwitchToVersion(version);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public ReaderWriterLockSlim GetReadWriteLock()
        {
            return l;
        }

        public bool IsSynchronizedDisk
        {
            get
            {
                l.EnterUpgradeableReadLock();
                try
                {
                    return o.IsSynchronizedDisk;
                }
                finally
                {
                    l.ExitUpgradeableReadLock();
                }
            }
        }

        public void MakeSynchronizedDisk(int id)
        {
            l.EnterWriteLock();
            try
            {
                o.MakeSynchronizedDisk(id);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public byte[] ReadBlock(long blockNumber)
        {
            l.EnterUpgradeableReadLock();
            try
            {
                return o.ReadBlock(blockNumber);
            }
            finally
            {
                l.ExitUpgradeableReadLock();
            }
        }

        public void WriteBlock(long blockNumber, byte[] block)
        {
            l.EnterWriteLock();
            try
            {
                o.WriteBlock(blockNumber, block);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void WriteFileSystemOptions(byte[] serializedFileSystemOptions)
        {
            l.EnterWriteLock();
            try
            {
                o.WriteFileSystemOptions(serializedFileSystemOptions);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void WriteConfig()
        {
            l.EnterWriteLock();
            try
            {
                o.WriteConfig();
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public void Reload(FileSystemOptions newOptions)
        {
            l.EnterWriteLock();
            try
            {
                o.Reload(newOptions);
            }
            finally
            {
                l.ExitWriteLock();
            }
        }

        public event EventHandler<FileSystemChangedEventArgs> FileSystemChanged
        {
            add { o.FileSystemChanged += value; }
            remove { o.FileSystemChanged -= value; }
        }

        public void OnFileSystemChanged(object sender, FileSystemChangedEventArgs e)
        {
            l.EnterWriteLock();
            try
            {
                o.OnFileSystemChanged(sender, e);
            }
            finally
            {
                l.ExitWriteLock();
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

            o.Dispose();
        }
    }
}
