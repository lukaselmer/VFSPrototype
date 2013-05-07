using System;
using System.Runtime.Serialization;
using SQLite;

namespace VFSWCFService.DiskService
{
    [DataContract]
    public class DiskDto
    {
        [PrimaryKey]
        [DataMember]
        public string Uuid { get; set; }

        [DataMember]
        public long LastServerVersion { get; set; }

        [DataMember]
        public long LocalVersion { get; set; }

        [DataMember]
        public long NewestBlock { get; set; }

        [DataMember]
        public string UserLogin { get; set; }
    }
}