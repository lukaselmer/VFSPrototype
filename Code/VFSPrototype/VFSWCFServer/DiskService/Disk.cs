using System;
using System.Runtime.Serialization;
using VFSWCFService.UserService;

namespace VFSWCFService.DiskService
{
    [Serializable]
    [DataContract]
    public class Disk
    {
        [DataMember]
        public string Uuid { get; set; }

        [DataMember]
        public User User { get; set; }

        [DataMember]
        public long LastServerVersion { get; set; }

        [DataMember]
        public long LocalVersion { get; set; }

        [DataMember]
        public long NewestBlock { get; set; }
    }
}