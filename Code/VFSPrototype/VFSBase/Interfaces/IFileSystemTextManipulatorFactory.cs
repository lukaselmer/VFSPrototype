using VFSBase.DiskServiceReference;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    public interface IFileSystemTextManipulatorFactory
    {
        IFileSystemTextManipulator CreateFileSystemTextManipulator(FileSystemOptions options, string password);
        IFileSystemTextManipulator OpenFileSystemTextManipulator(string location, string password);
        IFileSystemTextManipulator LinkFileSystemTextManipulator(UserDto user, DiskDto disk, string location);
    }
}