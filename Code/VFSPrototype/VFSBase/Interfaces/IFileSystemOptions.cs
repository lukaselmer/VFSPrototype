namespace VFSBase.Interfaces
{
    public interface IFileSystemOptions
    {
        string Location { get; set; }
        long DiskSize { get; set; }
        uint MasterBlockSize { get; set; }
        int BlockSize { get; }
        long DiskFree { get; }
        long DiskOccupied { get; }
    }
}
