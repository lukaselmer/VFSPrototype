using System.Runtime.Serialization;

namespace VFSWCFService.DiskService
{
    /// <summary>
    /// The disk options are used to create the disk on the server and on the client (initial synchronization).
    /// </summary>
    [DataContract]
    public class DiskOptions
    {
        [DataMember]
        int BlockSize { get; set; }

        [DataMember]
        int MasterBlockSize { get; set; }

        [DataMember]
        byte[] SerializedFileSystemOptions { get; set; }
    }
}