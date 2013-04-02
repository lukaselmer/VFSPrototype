namespace VFSBase.Interfaces
{
    internal interface IFileSystemOptions
    {
        string Location { get; set; }
        ulong DiskSize { get; set; }
        uint MasterBlockSize { get; set; }
        int BlockSize { get; }
        ulong DiskFree { get; }
        ulong DiskOccupied { get; }
    }
}