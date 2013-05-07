using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using SQLite;
using VFSWCFContracts.DataTransferObjects;

namespace VFSWCFService.Persistence
{
    /// <summary>
    /// The persistence is responsible for the data persistence.
    /// </summary>
    public class Persistence : IDisposable
    {
        private string _pathToDbFileToDbFile;
        public string PathToDataStore { get; private set; }

        private SQLiteConnection _db;

        internal Persistence(string pathToDbFile)
        {
            InitDatabase(pathToDbFile);
        }

        private void InitDatabase(string pathToDbFile)
        {
            _pathToDbFileToDbFile = pathToDbFile;
            _db = File.Exists(_pathToDbFileToDbFile) ? OpenDatabase() : CreateDatabase();
            CreateTables();
        }

        public Persistence()
        {
            var p = HostingEnvironment.ApplicationPhysicalPath;
            PathToDataStore = Path.Combine(p, "App_Data");
            var pathToDbFileToDbFile = string.Format("{0}/data.sqlite", PathToDataStore);

            InitDatabase(pathToDbFileToDbFile);
        }

        private SQLiteConnection OpenDatabase()
        {
            return new SQLiteConnection(_pathToDbFileToDbFile, SQLiteOpenFlags.ReadWrite);
        }

        private SQLiteConnection CreateDatabase()
        {
            return new SQLiteConnection(_pathToDbFileToDbFile);
        }


        private void CreateTables()
        {
            _db.CreateTable<UserDto>();
            _db.CreateTable<DiskDto>();
            _db.CreateTable<DiskOptionsDto>();
        }

        /// <summary>
        /// Checks if the user with the specified login exists.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        internal bool LoginFree(string login)
        {
            var x = _db.Table<UserDto>().ToList();
            return _db.Find<UserDto>(d => d.Login == login) == null;
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

        public IList<DiskDto> Disks(UserDto userDto)
        {
            return _db.Table<DiskDto>().Where(d => d.UserId == userDto.Id).ToList();
        }

        public void CreateDisk(UserDto userDto, DiskDto diskDto)
        {
            if (FindUser(userDto.Login) == null) return;
            if (FindDisk(diskDto) != null) throw new Exception("duplicate id");
            _db.Insert(diskDto);
        }

        public void UpdateDisk(DiskDto diskDto)
        {
            _db.Update(diskDto);
        }

        public bool RemoveDisk(DiskDto diskDto)
        {
            if (FindDisk(diskDto) == null) return false;

            _db.Delete<DiskDto>(diskDto.Id);
            return true;
        }

        public DiskDto FindDisk(DiskDto remoteDiskDto)
        {
            return _db.Find<DiskDto>(remoteDiskDto.Id);
        }

        public DiskOptionsDto LoadDiskOptions(int id)
        {
            return _db.Find<DiskOptionsDto>(id);
        }

        public void SaveDiskOptions(int id, DiskOptionsDto optionsDto)
        {
            var disk = _db.Find<DiskDto>(d => d.Id == id);
            optionsDto.DiskId = disk.Id;
            _db.Insert(optionsDto);
        }

        public void Clear()
        {
            _db.DropTable<DiskDto>();
            _db.DropTable<UserDto>();
            _db.DropTable<DiskOptionsDto>();
            CreateTables();
        }

        public bool UserExists(UserDto userDto)
        {
            return _db.Find<UserDto>(userDto.Id) != null;
        }

        public UserDto Authenticate(string login, string hashedPassword)
        {
            return _db.Find<UserDto>(u => u.Login == login && u.HashedPassword == hashedPassword);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }
    }
}