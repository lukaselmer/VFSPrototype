using VFSBase.Interfaces;

namespace VFSBase.Implementation
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
            fileSystem.TestEncryptionKey();
            return new FileSystemTextManipulator(fileSystem);
        }
    }
}