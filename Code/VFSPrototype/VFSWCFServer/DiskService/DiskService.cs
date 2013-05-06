using System;
using System.Collections.Generic;
using System.IO;
using VFSBlockAbstraction;
using VFSWCFService.Common;
using VFSWCFService.UserService;

namespace VFSWCFService.DiskService
{
    public class DiskService : IDiskService
    {
        internal Persistence Persistence { get; set; }

        public DiskService()
        {
            Persistence = new Persistence();
        }

        public IList<Disk> Disks(User user)
        {
            if (!Persistence.UserExists(user.Login)) return new List<Disk>();
            if (Persistence.Disks(user) == null) return new List<Disk>();

            return Persistence.Disks(user);
        }

        public Disk CreateDisk(User user, DiskOptions options)
        {
            if (!Persistence.UserExists(user.Login)) return null;

            var d = new Disk { User = user, Uuid = Guid.NewGuid().ToString() };
            Persistence.CreateDisk(user, d);
            return d;
        }

        public bool DeleteDisk(Disk disk)
        {
            var disks = Persistence.Disks(disk.User);
            if (disks == null) return false;
            return Persistence.RemoveDisk(disk);
        }

        public SynchronizationState FetchSynchronizationState(Disk disk)
        {
            var serverDisk = Persistence.FindDisk(disk);

            var localChanges = disk.LastServerVersion < disk.LocalVersion;
            var serverChanges = disk.LastServerVersion < serverDisk.LocalVersion;

            if (localChanges) return serverChanges ? SynchronizationState.Conflicted : SynchronizationState.LocalChanges;
            return serverChanges ? SynchronizationState.RemoteChanges : SynchronizationState.UpToDate;
        }

        public DiskOptions GetDiskOptions(Disk disk)
        {
            return Persistence.LoadDiskOptions(disk.Uuid);
        }

        public void SetDiskOptions(Disk disk, DiskOptions options)
        {
            Persistence.SaveDiskOptions(disk.Uuid, options);
        }

        public void WriteBlock(string diskUuid, long blockNr, byte[] content)
        {
            var b = GetBlockManipulator(diskUuid);
            b.WriteBlock(blockNr, content);
        }

        private BlockManipulator GetBlockManipulator(string diskUuid)
        {
            var options = Persistence.LoadDiskOptions(diskUuid);
            var b = new BlockManipulator(DiskLocation(diskUuid), options.BlockSize, options.MasterBlockSize);
            return b;
        }

        private static string DiskLocation(string diskUuid)
        {
            if (!Directory.Exists("./Disks")) Directory.CreateDirectory("./Disks");
            return string.Format("./Disks/{0}.vhs", diskUuid);
        }

        public byte[] ReadBlock(string diskUuid, long blockNr)
        {
            var b = GetBlockManipulator(diskUuid);
            return b.ReadBlock(blockNr);
        }

        public void UpdateDisk(Disk disk)
        {
            Persistence.UpdateDisk(disk);
        }
    }
}