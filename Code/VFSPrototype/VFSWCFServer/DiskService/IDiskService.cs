using System.Collections.Generic;
using System.ServiceModel;
using VFSWCFService.UserService;

namespace VFSWCFService.DiskService
{
    [ServiceContract]
    public interface IDiskService
    {
        [OperationContract]
        IList<Disk> Disks(User user);

        [OperationContract]
        Disk CreateDisk(User user);

        [OperationContract]
        bool DeleteDisk(Disk disk);

        [OperationContract]
        SynchronizationState FetchSynchronizationState(Disk disk);
    }
}
