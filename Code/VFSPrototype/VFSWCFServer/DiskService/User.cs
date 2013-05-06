using System;
using System.Runtime.Serialization;

namespace VFSWCFService.UserService
{
    [Serializable]
    [DataContract]
    public class User
    {
        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string HashedPassword { get; set; }
    }
}