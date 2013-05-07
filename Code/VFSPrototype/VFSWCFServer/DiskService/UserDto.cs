using System;
using System.Runtime.Serialization;
using SQLite;

namespace VFSWCFService.DiskService
{
    [Serializable]
    [DataContract]
    public class UserDto
    {
        [PrimaryKey]
        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string HashedPassword { get; set; }
    }
}