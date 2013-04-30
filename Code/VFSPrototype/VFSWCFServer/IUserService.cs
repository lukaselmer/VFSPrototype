using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace VFSWCFServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IUserService" in both code and config file together.
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        User Register(string login, string hashedPassword);

        [OperationContract]
        User Login(string login, string hashedPassword);
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class User
    {
        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string HashedPassword { get; set; }
    }
}
