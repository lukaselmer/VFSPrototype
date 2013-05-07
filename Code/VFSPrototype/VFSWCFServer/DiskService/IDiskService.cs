using System.Collections.Generic;
using System.ServiceModel;
using VFSWCFContracts.Contracts;
using VFSWCFService.DataTransferObjects;

namespace VFSWCFService.DiskService
{
    [ServiceContract]
    public interface IDiskService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        UserDto Register(string login, string hashedPassword);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        UserDto Login(string login, string hashedPassword);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        IList<DiskDto> Disks(UserDto userDto);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        DiskDto CreateDisk(UserDto userDto, DiskOptionsDto optionsDto);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        bool DeleteDisk(UserDto userDto, DiskDto diskDto);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        SynchronizationState FetchSynchronizationState(UserDto userDto, DiskDto diskDto);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        DiskOptionsDto GetDiskOptions(UserDto userDto, DiskDto diskDto);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        void SetDiskOptions(UserDto userDto, DiskDto diskDto, DiskOptionsDto optionsDto);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        void WriteBlock(UserDto userDto, int diskId, long blockNr, byte[] content);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        byte[] ReadBlock(UserDto userDto, int diskId, long blockNr);

        [OperationContract]
        [FaultContract(typeof(ServiceException))]
        void UpdateDisk(UserDto userDto, DiskDto diskDto);
    }
}
