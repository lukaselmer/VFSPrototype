using System;
using VFSBase.DiskServiceReference;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Synchronization;

namespace VFSBase.Factories
{
    internal class FileSystemTextManipulatorFactory : IFileSystemTextManipulatorFactory
    {
        public IFileSystemTextManipulator CreateFileSystemTextManipulator(FileSystemOptions options, string password)
        {
            var fileSystem = FileSystemFactory.Create(options, password);
            fileSystem.TestEncryptionKey();
            return new FileSystemTextManipulator(fileSystem);
        }

        public IFileSystemTextManipulator OpenFileSystemTextManipulator(string location, string password)
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
            return new FileSystemTextManipulator(fileSystem);
        }

        public IFileSystemTextManipulator LinkFileSystemTextManipulator(UserDto user, DiskDto disk, string location)
        {
            //TODO: implement thsi
            throw new NotImplementedException();
        }
    }
}