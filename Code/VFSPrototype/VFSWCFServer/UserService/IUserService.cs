using System.ServiceModel;

namespace VFSWCFService.UserService
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
}
