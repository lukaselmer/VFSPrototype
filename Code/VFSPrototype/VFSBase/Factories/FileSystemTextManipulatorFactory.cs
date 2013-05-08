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
            //var x = SynchronizationService.CreateService(fileSystem, new User(), new SynchronizationCallbacks(null));

            new SynchronizationService(fileSystem, new UserDto(), new SynchronizationCallbacks(null));

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
    }
}