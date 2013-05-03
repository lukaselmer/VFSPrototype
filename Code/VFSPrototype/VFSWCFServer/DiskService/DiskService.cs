using System;
using System.Collections.Generic;
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
            if(!Persistence.UserExists(user.Login)) return new List<Disk>();
            if(Persistence.Disks(user) == null) return new List<Disk>();

            return Persistence.Disks(user);
        }

        public Disk CreateDisk(User user)
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
            return disks.Remove(disk);
        }
    }
}