using System.Collections.Generic;
using System.ServiceModel;

namespace VFSWCFService.DiskService
{
    [ServiceContract]
    public interface IDiskService
    {
        [OperationContract]
        UserDto Register(string login, string hashedPassword);

        [OperationContract]
        UserDto Login(string login, string hashedPassword);

        [OperationContract]
        IList<DiskDto> Disks(UserDto userDto);

        [OperationContract]
        DiskDto CreateDisk(UserDto userDto, DiskOptions options);

        [OperationContract]
        bool DeleteDisk(DiskDto diskDto);

        [OperationContract]
        SynchronizationState FetchSynchronizationState(DiskDto diskDto);

        [OperationContract]
        DiskOptions GetDiskOptions(DiskDto diskDto);

        [OperationContract]
        void SetDiskOptions(DiskDto diskDto, DiskOptions options);

        [OperationContract]
        void WriteBlock(string diskUuid, long blockNr, byte[] content);

        [OperationContract]
        byte[] ReadBlock(string diskUuid, long blockNr);

        [OperationContract]
        void UpdateDisk(DiskDto diskDto);
    }
}
