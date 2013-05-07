using System.Runtime.Serialization;
using SQLite;

namespace VFSWCFService.DataTransferObjects
{
    [DataContract]
    public class DiskDto
    {
        [PrimaryKey]
        [AutoIncrement]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public long LastServerVersion { get; set; }

        [DataMember]
        public long LocalVersion { get; set; }

        [DataMember]
        public long NewestBlock { get; set; }
    }
}