using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using VFSBase.DiskServiceReference;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Synchronization;
using VFSBlockAbstraction;

namespace VFSBase.Factories
{
    internal class FileSystemTextManipulatorFactory : IFileSystemTextManipulatorFactory
    {
        public IFileSystemTextManipulator Create(FileSystemOptions options, string password)
        {
            var fileSystem = FileSystemFactory.Create(options, password);
            fileSystem.TestEncryptionKey();
            return new ThreadSafeFileSystemTextManipulator(fileSystem);
        }

        public IFileSystemTextManipulator Open(string location, string password)
        {
            var fileSystem = FileSystemFactory.Import(location, password);
            try
            {
                fileSystem.TestEncryptionKey();
            }
            catch
            {
                fileSystem.Dispose();
                throw;
            }
            return new ThreadSafeFileSystemTextManipulator(fileSystem);
        }

        public void Link(DiskOptionsDto diskOptions, string location)
        {
            if (diskOptions == null) throw new ArgumentNullException("diskOptions");

            if (File.Exists(location)) File.Delete(location);

            FileSystemOptions fileSystemOptions;

            using (var ms = new MemoryStream())
            {
                ms.Write(diskOptions.SerializedFileSystemOptions, 0, diskOptions.SerializedFileSystemOptions.Length);
                ms.Seek(0, SeekOrigin.Begin);

                IFormatter formatter = new BinaryFormatter();
                fileSystemOptions = formatter.Deserialize(ms) as FileSystemOptions;
                if (fileSystemOptions == null) throw new VFSException("Invalid remote file");

                fileSystemOptions.LocalVersion = 0;
                fileSystemOptions.LastServerVersion = 0;
                fileSystemOptions.RootBlockNr = 0;
            }

            using (var disk = File.OpenWrite(location))
            {
                IFormatter f = new BinaryFormatter();
                f.Serialize(disk, fileSystemOptions);
            }
        }
    }
}