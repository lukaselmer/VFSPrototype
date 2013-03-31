namespace VFSBase.Interfaces
{
    public interface IFileSystem
    {
        ulong DiskSize { get; }
        ulong DiskFree { get; }
        ulong DiskOccupied { get; }
        string Location { get; }
        void Destroy();
    }
}