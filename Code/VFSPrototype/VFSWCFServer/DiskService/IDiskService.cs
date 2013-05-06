using System.Collections.Generic;
using System.ServiceModel;
using VFSWCFService.UserService;

namespace VFSWCFService.DiskService
{
    [ServiceContract]
    public interface IDiskService
    {
        [OperationContract]
        User Register(string login, string hashedPassword);

        [OperationContract]
        User Login(string login, string hashedPassword);

        [OperationContract]
        IList<Disk> Disks(User user);

        [OperationContract]
        Disk CreateDisk(User user, DiskOptions options);

        [OperationContract]
        bool DeleteDisk(Disk disk);

        [OperationContract]
        SynchronizationState FetchSynchronizationState(Disk disk);

        [OperationContract]
        DiskOptions GetDiskOptions(Disk disk);

        [OperationContract]
        void SetDiskOptions(Disk disk, DiskOptions options);

        [OperationContract]
        void WriteBlock(string diskUuid, long blockNr, byte[] content);

        [OperationContract]
        byte[] ReadBlock(string diskUuid, long blockNr);

        [OperationContract]
        void UpdateDisk(Disk disk);
    }
}
