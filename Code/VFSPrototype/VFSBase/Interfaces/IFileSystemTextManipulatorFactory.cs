using VFSBase.DiskServiceReference;
using VFSBase.Implementation;

namespace VFSBase.Interfaces
{
    public interface IFileSystemTextManipulatorFactory
    {
        IFileSystemTextManipulator Create(FileSystemOptions options, string password);
        IFileSystemTextManipulator Open(string location, string password);
        void Link(DiskOptionsDto diskOptions, string location);
    }
}