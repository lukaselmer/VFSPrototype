using System.Runtime.Serialization;

namespace VFSWCFContracts.FaultContracts
{
    [DataContract]
    public class ServiceFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}
