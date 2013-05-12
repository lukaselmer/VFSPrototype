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
    public sealed class DiskServiceImpl : IDiskService, IDisposable
    {
        private Persistence.PersistenceImpl Persistence { get; set; }

        public DiskServiceImpl() : this(new Persistence.PersistenceImpl()) { }

        internal DiskServiceImpl(Persistence.PersistenceImpl persistence)
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

            return Persistence.CreateDisk(userDto, optionsDto);
        }

        public bool DeleteDisk(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);
            Authenticate(userDto, diskDto);

            var disks = Persistence.Disks(userDto);
            if (disks == null) return false;
            return Persistence.RemoveDisk(diskDto);
        }

        public SynchronizationState FetchSynchronizationState(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);
            Authenticate(userDto, diskDto);

            var serverDisk = Persistence.FindDisk(diskDto);

            var localChanges = diskDto.LastServerVersion < diskDto.LocalVersion;
            var serverChanges = diskDto.LastServerVersion < serverDisk.LocalVersion;

            if (localChanges) return serverChanges ? SynchronizationState.Conflicted : SynchronizationState.LocalChanges;
            return serverChanges ? SynchronizationState.RemoteChanges : SynchronizationState.UpToDate;
        }

        public DiskOptionsDto GetDiskOptions(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);
            Authenticate(userDto, diskDto);

            return Persistence.LoadDiskOptions(diskDto.Id);
        }

        public void SetDiskOptions(UserDto userDto, DiskDto diskDto, DiskOptionsDto optionsDto)
        {
            Authenticate(userDto);
            Authenticate(userDto, diskDto);

            Persistence.SaveDiskOptions(diskDto.Id, optionsDto);
        }

        public void WriteBlock(UserDto userDto, int diskId, long blockNr, byte[] content)
        {
            Authenticate(userDto);
            Authenticate(userDto, new DiskDto { Id = diskId });

            using (var b = GetBlockManipulator(diskId))
            {
                b.WriteBlock(blockNr, content);
            }
        }

        public byte[] ReadBlock(UserDto userDto, int diskId, long blockNr)
        {
            Authenticate(userDto);
            Authenticate(userDto, new DiskDto { Id = diskId });

            using (var b = GetBlockManipulator(diskId))
            {
                return b.ReadBlock(blockNr);
            }
        }

        public void UpdateDisk(UserDto userDto, DiskDto diskDto)
        {
            Authenticate(userDto);
            Authenticate(userDto, diskDto);

            Persistence.UpdateDisk(diskDto);
        }

        private BlockManipulator GetBlockManipulator(int id)
        {
            var options = Persistence.LoadDiskOptions(id);
            var path = DiskLocation(id);

            if (!File.Exists(path)) File.WriteAllText(path, "");

            var b = new BlockManipulator(path, options.BlockSize, options.MasterBlockSize);
            return b;
        }

        private string DiskLocation(int id)
        {
            var location = Path.Combine(Persistence.PathToDataStore, "Disks");
            if (!Directory.Exists(location)) Directory.CreateDirectory(location);
            return Path.Combine(location, string.Format("{0}.vhs", id));
        }

        /// <summary>
        /// Registers a user with the specified login and password.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>If successful, the user, null otherwise.</returns>
        public UserDto Register(string login, string hashedPassword)
        {
            if (string.IsNullOrEmpty(login) || login.Length < 3) ErrorOccured("Login must contain at least 3 characters");
            if (string.IsNullOrEmpty(hashedPassword)) ErrorOccured("Password must not be empty");

            if (!Persistence.LoginFree(login)) ErrorOccured(string.Format("Login \"{0}\" taken.", login));

            var user = Persistence.CreateUser(login, hashedPassword);

            return user;
        }

        private void Authenticate(UserDto userDto)
        {
            var user = Login(userDto.Login, userDto.HashedPassword);
            userDto.Id = user.Id;
        }

        private void Authenticate(UserDto userDto, DiskDto diskDto)
        {
            var disk = Persistence.Disk(diskDto);
            diskDto.UserId = disk.UserId;
            if (diskDto.UserId != userDto.Id) ErrorOccured(string.Format("You don't have access to the disk {0}", diskDto.Id));
        }

        /// <summary>
        /// Authenticates the user with the login and password.
        /// </summary>
        /// <param name="loginName">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>The user if login is successful, null otherwise.</returns>
        public UserDto Login(string loginName, string hashedPassword)
        {
            var user = Persistence.Authenticate(loginName, hashedPassword);

            if (user == null) ErrorOccured("Authentication failed. Please check username and password.");

            return user;
        }

        private static void ErrorOccured(string message)
        {
            throw new FaultException<ServiceFault>(new ServiceFault { Message = message }, new FaultReason(message));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (Persistence != null)
            {
                Persistence.Dispose();
                Persistence = null;
            }
        }
    }
}