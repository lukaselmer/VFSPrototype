﻿using System.Collections.Generic;
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
        Disk CreateDisk(User user, DiskOptions options);

        [OperationContract]
        bool DeleteDisk(Disk disk);

        [OperationContract]
        SynchronizationState FetchSynchronizationState(Disk disk);

        [OperationContract]
        DiskOptions GetDiskOptions(Disk disk);

        [OperationContract]
        void SetDiskOptions(DiskOptions disk);

        [OperationContract]
        void WriteBlock(string diskUuid, long blockNr, byte[] content);

        [OperationContract]
        byte[] ReadBlock(string diskUuid, long blockNr);
    }
}
