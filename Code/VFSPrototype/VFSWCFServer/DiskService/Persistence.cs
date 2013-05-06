using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Hosting;
using SQLite;

namespace VFSWCFService.DiskService
{
    /// <summary>
    /// The persistence is responsible for the data persistence.
    /// </summary>
    public class Persistence
    {
        private Dictionary<string, UserDto> _userStorage = new Dictionary<string, UserDto>();
        private Dictionary<string, Dictionary<string, DiskDto>> _diskStorage = new Dictionary<string, Dictionary<string, DiskDto>>();
        private Dictionary<string, DiskOptions> _diskOptions = new Dictionary<string, DiskOptions>();

        private readonly string _pathToDataStore;
        private readonly string _pathToDbFile;

        private SQLiteConnection _db;

        public Persistence()
        {
            var p = HostingEnvironment.ApplicationPhysicalPath ?? "../../Testfiles";
            _pathToDataStore = Path.Combine(p, "App_Data");
            _pathToDbFile = string.Format("{0}/data.sqlite", _pathToDataStore);


            if (!Directory.Exists(_pathToDataStore)) Directory.CreateDirectory(_pathToDataStore);

            _db = new SQLiteConnection(_pathToDbFile);
            if (_db.TableMappings.Count() != 3) CreateTables();
        }

        private void CreateTables()
        {
            _db.CreateTable<UserDto>();
            _db.CreateTable<DiskDto>();
            _db.CreateTable<DiskOptions>();
        }

        /// <summary>
        /// Checks if the user with the specified login exists.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        public bool UserExists(string login)
        {
            return FindUser(login) != null;
        }

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns></returns>
        public UserDto CreateUser(string login, string hashedPassword)
        {
            var u = new UserDto { Login = login, HashedPassword = hashedPassword };
            _db.Insert(u);
            return u;
        }

        /// <summary>
        /// Finds the user.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        internal UserDto FindUser(string login)
        {
            return _db.Find<UserDto>(u => u.Login == login);
        }

        public IList<DiskDto> Disks(string userLogin)
        {
            return _db.Table<DiskDto>().Where(d => d.UserLogin == userLogin).ToList();
        }

        public void CreateDisk(UserDto userDto, DiskDto diskDto)
        {
            if (FindUser(userDto.Login) == null) return;
            if (FindDisk(diskDto) != null) throw new Exception("duplicate uuid"); ;
            _db.Insert(diskDto);
        }

        public void UpdateDisk(DiskDto diskDto)
        {
            _db.Update(diskDto);
        }

        public bool RemoveDisk(DiskDto diskDto)
        {
            if (FindDisk(diskDto) == null) return false;

            _db.Delete<DiskDto>(diskDto.Uuid);
            return true;
        }

        public DiskDto FindDisk(DiskDto remoteDiskDto)
        {
            return _db.Find<DiskDto>(d => d.Uuid == remoteDiskDto.Uuid);
        }

        public DiskOptions LoadDiskOptions(string uuid)
        {
            return _db.Find<DiskOptions>(uuid);
        }

        public void SaveDiskOptions(string uuid, DiskOptions options)
        {
            options.DiskUuid = uuid;
            _db.Insert(options);
        }

        public void Clear()
        {
            _db.DropTable<DiskDto>();
            _db.DropTable<UserDto>();
            _db.DropTable<DiskOptions>();
            CreateTables();
        }
    }
}