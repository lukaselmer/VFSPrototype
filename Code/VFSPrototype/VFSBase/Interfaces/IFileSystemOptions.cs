namespace VFSBase.Interfaces
{
    internal interface IFileSystemOptions
    {
        string Location { get; set; }
        ulong DiskSize { get; set; }
        uint MasterBlockSize { get; set; }
        ulong DiskFree { get; }
        ulong DiskOccupied { get; }
    }
}