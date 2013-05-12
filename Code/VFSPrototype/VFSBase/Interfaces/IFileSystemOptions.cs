namespace VFSBase.Interfaces
{
    public interface IFileSystemOptions
    {
        string Location { get; set; }
        int MasterBlockSize { get; set; }
        int BlockSize { get; }
        long DiskFree { get; }
        long DiskOccupied { get; }
    }
}
