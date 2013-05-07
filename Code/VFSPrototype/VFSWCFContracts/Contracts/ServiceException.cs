using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VFSWCFContracts.Contracts
{
    [DataContract]
    public class ServiceException
    {
        [DataMember]
        public string Message { get; set; }
    }
}
