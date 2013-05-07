using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using VFSBlockAbstraction;
using VFSWCFContracts.Contracts;
using VFSWCFContracts.DataTransferObjects;
using VFSWCFContracts.FaultContracts;

namespace VFSWCFService.DiskService
{
    public class DiskService : IDiskService
    {
        private Persistence.Persistence Persistence { get; set; }

        public DiskService() : this(new Persistence.Persistence()) { }

        internal DiskService(Persistence.Persistence persistence)
        {
            Persistence = persistence;
        }

        public IList<DiskDto> Disks(UserDto userDto)
        {
            Authenticate(userDto);

            return Persistence.Disks(userDto);
        }

        public DiskDto CreateDisk(UserDto userDto, DiskOptionsDto optionsDto)
        {
            Authenticate(userDto);

            var d = new DiskDto { UserId = userDto.Id };
            Persistence.CreateDisk(userDto, d);
            return d;
        }

        public bool DeleteDisk(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);

            var disks = Persistence.Disks(userDto);
            if (disks == null) return false;
            return Persistence.RemoveDisk(diskDto);
        }

        public SynchronizationState FetchSynchronizationState(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);

            var serverDisk = Persistence.FindDisk(diskDto);

            var localChanges = diskDto.LastServerVersion < diskDto.LocalVersion;
            var serverChanges = diskDto.LastServerVersion < serverDisk.LocalVersion;

            if (localChanges) return serverChanges ? SynchronizationState.Conflicted : SynchronizationState.LocalChanges;
            return serverChanges ? SynchronizationState.RemoteChanges : SynchronizationState.UpToDate;
        }

        public DiskOptionsDto GetDiskOptions(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);

            return Persistence.LoadDiskOptions(diskDto.Id);
        }

        public void SetDiskOptions(UserDto userDto, DiskDto diskDto, DiskOptionsDto optionsDto)
        {
            Authenticate(userDto);

            Persistence.SaveDiskOptions(diskDto.Id, optionsDto);
        }

        public void WriteBlock(UserDto userDto, int diskId, long blockNr, byte[] content)
        {
            Authenticate(userDto);

            var b = GetBlockManipulator(diskId);
            b.WriteBlock(blockNr, content);
        }

        private BlockManipulator GetBlockManipulator(int id)
        {
            var options = Persistence.LoadDiskOptions(id);
            var b = new BlockManipulator(DiskLocation(id), options.BlockSize, options.MasterBlockSize);
            return b;
        }

        private string DiskLocation(int id)
        {
            var location = Path.Combine(Persistence.PathToDataStore, "Disks");
            if (!Directory.Exists(location)) Directory.CreateDirectory(location);
            return Path.Combine(location, string.Format("{0}.vhs", id));
        }

        public byte[] ReadBlock(UserDto userDto, int diskId, long blockNr)
        {
            Authenticate(userDto);

            var b = GetBlockManipulator(diskId);
            return b.ReadBlock(blockNr);
        }

        public void UpdateDisk(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);

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
            return Persistence.LoginFree(login) ? Persistence.CreateUser(login, hashedPassword) : null;
        }

        private void Authenticate(UserDto userDto)
        {
            Login(userDto.Login, userDto.HashedPassword);
        }

        /// <summary>
        /// Authenticates the user with the login and password.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>The user if login is successful, null otherwise.</returns>
        public UserDto Login(string login, string hashedPassword)
        {
            var user = Persistence.Authenticate(login, hashedPassword);

            if (user == null) throw new FaultException<ServiceFault>(
                new ServiceFault { Message = "Authentication failed. Please check username and password." });

            return user;
        }
    }
}