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

        // CA1051 does not apply to data contracts. In data contracts, instance fields may be public.
        // See also: http://msdn.microsoft.com/en-us/library/aa347850.aspx
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [DataMember]
        public byte[] SerializedFileSystemOptions;
    }
}