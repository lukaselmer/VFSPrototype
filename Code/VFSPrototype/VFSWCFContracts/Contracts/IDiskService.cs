using System.Collections.Generic;
using System.ServiceModel;
using VFSWCFContracts.DataTransferObjects;
using VFSWCFContracts.FaultContracts;

namespace VFSWCFContracts.Contracts
{
    [ServiceContract]
    public interface IDiskService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UserDto Register(string login, string hashedPassword);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UserDto Login(string loginName, string hashedPassword);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IList<DiskDto> Disks(UserDto userDto);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DiskDto CreateDisk(UserDto userDto, DiskOptionsDto optionsDto);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool DeleteDisk(UserDto userDto, DiskDto diskDto);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        SynchronizationState FetchSynchronizationState(UserDto userDto, DiskDto diskDto);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DiskOptionsDto GetDiskOptions(UserDto userDto, DiskDto diskDto);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SetDiskOptions(UserDto userDto, DiskDto diskDto, DiskOptionsDto optionsDto);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void WriteBlock(UserDto userDto, int diskId, long blockNr, byte[] content);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        byte[] ReadBlock(UserDto userDto, int diskId, long blockNr);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void UpdateDisk(UserDto userDto, DiskDto diskDto);
    }
}
