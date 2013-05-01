using System.ServiceModel;

namespace VFSWCFService.DiskService
{
    [ServiceContract]
    public interface IDiskService
    {
        [OperationContract]
        string Ish(string input);
    }
}
