using System.Runtime.Serialization;
using SQLite;

namespace VFSWCFContracts.DataTransferObjects
{
    /// <summary>
    /// The disk options are used to create the disk on the server and on the client (initial synchronization).
    /// </summary>
    [DataContract]
    public class DiskOptionsDto
    {
        [PrimaryKey]
        [AutoIncrement]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int DiskId { get; set; }

        [DataMember]
        public int BlockSize { get; set; }

        [DataMember]
        public int MasterBlockSize { get; set; }

        [DataMember]
        public byte[] SerializedFileSystemOptions { get; set; }
    }
}