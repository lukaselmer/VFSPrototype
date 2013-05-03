using System.Runtime.Serialization;
using VFSWCFService.UserService;

namespace VFSWCFService.DiskService
{
    [DataContract]
    public class Disk
    {
        [DataMember]
        public string Uuid { get; set; }

        [DataMember]
        public User User { get; set; }
    }
}