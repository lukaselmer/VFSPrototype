using System;
using System.Collections.Generic;
using System.IO;
using VFSBlockAbstraction;

namespace VFSWCFService.DiskService
{
    public class DiskService : IDiskService
    {
        internal Persistence Persistence { get; set; }

        public DiskService()
        {
            Persistence = new Persistence();
        }

        public IList<DiskDto> Disks(UserDto userDto)
        {
            if (!Persistence.UserExists(userDto.Login)) return new List<DiskDto>();
            if (Persistence.Disks(userDto.Login) == null) return new List<DiskDto>();

            return Persistence.Disks(userDto.Login);
        }

        public DiskDto CreateDisk(UserDto userDto, DiskOptions options)
        {
            if (!Persistence.UserExists(userDto.Login)) return null;

            var d = new DiskDto { UserLogin = userDto.Login, Uuid = Guid.NewGuid().ToString() };
            Persistence.CreateDisk(userDto, d);
            return d;
        }

        public bool DeleteDisk(DiskDto diskDto)
        {
            var disks = Persistence.Disks(diskDto.UserLogin);
            if (disks == null) return false;
            return Persistence.RemoveDisk(diskDto);
        }

        public SynchronizationState FetchSynchronizationState(DiskDto diskDto)
        {
            var serverDisk = Persistence.FindDisk(diskDto);

            var localChanges = diskDto.LastServerVersion < diskDto.LocalVersion;
            var serverChanges = diskDto.LastServerVersion < serverDisk.LocalVersion;

            if (localChanges) return serverChanges ? SynchronizationState.Conflicted : SynchronizationState.LocalChanges;
            return serverChanges ? SynchronizationState.RemoteChanges : SynchronizationState.UpToDate;
        }

        public DiskOptions GetDiskOptions(DiskDto diskDto)
        {
            return Persistence.LoadDiskOptions(diskDto.Uuid);
        }

        public void SetDiskOptions(DiskDto diskDto, DiskOptions options)
        {
            Persistence.SaveDiskOptions(diskDto.Uuid, options);
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

        public void UpdateDisk(DiskDto diskDto)
        {
            Persistence.UpdateDisk(diskDto);
        }

        /// <summary>
        /// Registers a user with the specified login and password.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>If successful, the user, null otherwise.</returns>
        public UserDto Register(string login, string hashedPassword)
        {
            return Persistence.UserExists(login) ? null : Persistence.CreateUser(login, hashedPassword);
        }

        /// <summary>
        /// Authenticates the user with the login and password.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>The user if login is successful, null otherwise.</returns>
        public UserDto Login(string login, string hashedPassword)
        {
            if (!Persistence.UserExists(login)) return null;

            var u = Persistence.FindUser(login);
            return u.HashedPassword == hashedPassword ? u : null;
        }
    }
}