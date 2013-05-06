using System;
using System.Collections.Generic;
using System.Linq;
using VFSWCFService.DiskService;
using VFSWCFService.UserService;

namespace VFSWCFService.Common
{
    /// <summary>
    /// The persistence is responsible for the data persistence.
    /// </summary>
    public class Persistence
    {
        private readonly Dictionary<string, User> _userStorage = new Dictionary<string, User>();
        private readonly Dictionary<string, Dictionary<string, Disk>> _diskStorage = new Dictionary<string, Dictionary<string, Disk>>();
        private Dictionary<string, DiskOptions> _diskOptions = new Dictionary<string, DiskOptions>();

        /// <summary>
        /// Checks if the user with the specified login exists.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        public bool UserExists(string login)
        {
            return _userStorage.ContainsKey(login);
        }

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns></returns>
        public User CreateUser(string login, string hashedPassword)
        {
            var u = new User { Login = login, HashedPassword = hashedPassword };
            _userStorage[login] = u;
            return u;
        }

        /// <summary>
        /// Finds the user.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        internal User FindUser(string login)
        {
            return _userStorage[login];
        }

        public IList<Disk> Disks(User user)
        {
            return _diskStorage.ContainsKey(user.Login) ? _diskStorage[user.Login].Values.ToList() : null;
        }

        public void CreateDisk(User user, Disk disk)
        {
            if (!_diskStorage.ContainsKey(user.Login)) _diskStorage[user.Login] = new Dictionary<string, Disk>();
            if (_diskStorage[user.Login].ContainsKey(disk.Uuid)) throw new Exception("duplicate uuid");
            _diskStorage[user.Login][disk.Uuid] = disk;
        }

        public void UpdateDisk(Disk disk)
        {
            _diskStorage[disk.User.Login][disk.Uuid] = disk;
        }

        public bool RemoveDisk(Disk disk)
        {
            if (!_diskStorage[disk.User.Login].ContainsKey(disk.Uuid)) return false;

            _diskStorage[disk.User.Login].Remove(disk.Uuid);
            return true;
        }

        public Disk FindDisk(Disk remoteDisk)
        {
            return _diskStorage[remoteDisk.User.Login][remoteDisk.Uuid];
        }

        public DiskOptions LoadDiskOptions(Disk disk)
        {
            return _diskOptions[disk.Uuid];
        }

        public void SaveDiskOptions(Disk disk, DiskOptions options)
        {
            _diskOptions[disk.Uuid] = options;
        }
    }
}