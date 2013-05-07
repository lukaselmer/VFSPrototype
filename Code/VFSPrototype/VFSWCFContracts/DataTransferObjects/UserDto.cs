using System;
using System.Runtime.Serialization;
using SQLite;

namespace VFSWCFContracts.DataTransferObjects
{
    [Serializable]
    [DataContract]
    public class UserDto
    {
        [PrimaryKey]
        [AutoIncrement]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string HashedPassword { get; set; }
    }
}