namespace VFSBase.Interfaces
{
    public interface IFileSystemData
    {
        ulong DiskSize { get; }
        ulong DiskFree { get; }
        ulong DiskOccupied { get; }
        string Location { get; }
    }
}