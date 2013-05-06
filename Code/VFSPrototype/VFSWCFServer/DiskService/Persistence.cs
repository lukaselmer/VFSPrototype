using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Hosting;
using VFSWCFService.UserService;

namespace VFSWCFService.DiskService
{
    /// <summary>
    /// The persistence is responsible for the data persistence.
    /// </summary>
    public class Persistence
    {
        private Dictionary<string, User> _userStorage = new Dictionary<string, User>();
        private Dictionary<string, Dictionary<string, Disk>> _diskStorage = new Dictionary<string, Dictionary<string, Disk>>();
        private Dictionary<string, DiskOptions> _diskOptions = new Dictionary<string, DiskOptions>();

        private readonly string _pathToDataStore;
        private readonly string _pathToSerializedFile;

        public Persistence()
        {
            return;
            var p = HostingEnvironment.ApplicationPhysicalPath ?? "../../Testfiles";
            _pathToDataStore = Path.Combine(p, "App_Data");
            _pathToSerializedFile = string.Format("{0}/data.serialized", _pathToDataStore);

            if (!Directory.Exists(_pathToDataStore)) Directory.CreateDirectory(_pathToDataStore);

            if (File.Exists(_pathToSerializedFile)) Import();
        }

        private void Import()
        {
            return;
            if (!File.Exists(_pathToSerializedFile)) return;

            IFormatter formatter = new BinaryFormatter();
            using (var stream = File.OpenRead(_pathToSerializedFile))
            {
                try
                {
                    _userStorage = formatter.Deserialize(stream) as Dictionary<string, User>;
                    _diskStorage = formatter.Deserialize(stream) as Dictionary<string, Dictionary<string, Disk>>;
                    _diskOptions = formatter.Deserialize(stream) as Dictionary<string, DiskOptions>;
                }
                catch (SerializationException)
                {
                    
                }
            }
        }

        private void Persist()
        {
            return;
            if (File.Exists(_pathToSerializedFile)) File.Delete(_pathToSerializedFile);

            IFormatter formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(_pathToSerializedFile))
            {
                formatter.Serialize(stream, _userStorage);
                formatter.Serialize(stream, _diskStorage);
                formatter.Serialize(stream, _diskOptions);
            }
        }

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
            Persist();
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
            Persist();
        }

        public void UpdateDisk(Disk disk)
        {
            _diskStorage[disk.User.Login][disk.Uuid] = disk;
            Persist();
        }

        public bool RemoveDisk(Disk disk)
        {
            if (!_diskStorage[disk.User.Login].ContainsKey(disk.Uuid)) return false;

            _diskStorage[disk.User.Login].Remove(disk.Uuid);
            Persist();
            return true;
        }

        public Disk FindDisk(Disk remoteDisk)
        {
            return _diskStorage[remoteDisk.User.Login][remoteDisk.Uuid];
        }

        public DiskOptions LoadDiskOptions(string uuid)
        {
            return _diskOptions[uuid];
        }

        public void SaveDiskOptions(string uuid, DiskOptions options)
        {
            _diskOptions[uuid] = options;
            Persist();
        }
    }
}