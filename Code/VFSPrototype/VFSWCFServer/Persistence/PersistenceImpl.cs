using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using SQLite;
using VFSWCFContracts.DataTransferObjects;

namespace VFSWCFService.Persistence
{
    /// <summary>
    /// The persistence is responsible for the data persistence.
    /// </summary>
    public class PersistenceImpl : IDisposable
    {
        private string _pathToDbFileToDbFile;
        public string PathToDataStore { get; private set; }

        private SQLiteConnection _db;

        internal PersistenceImpl(string pathToDbFile)
        {
            InitDatabase(pathToDbFile);
        }

        private void InitDatabase(string pathToDbFile)
        {
            _pathToDbFileToDbFile = pathToDbFile;
            try
            {
                _db = File.Exists(_pathToDbFileToDbFile) ? OpenDatabase() : CreateDatabase();
                CreateTables();
            }
            catch (DllNotFoundException e)
            {
                DisplaySqliteError(e);
            }
        }

        private static void DisplaySqliteError(DllNotFoundException e)
        {
            var currentDirectory = new DirectoryInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).PathAndQuery);
            while (currentDirectory.Parent != null)
            {
                var testPath = currentDirectory.Parent.FullName + "\\sqlite\\sqlite.dll";
                if (!File.Exists(testPath))
                {
                    currentDirectory = currentDirectory.Parent;
                    continue;
                }

                var message = string.Format("Please add {0} to your system path", currentDirectory.Parent.FullName + "\\sqlite");
                Console.WriteLine(e.Message);
                Console.WriteLine(message);
                throw new PersistenceException(message);
            }
            throw new PersistenceException("Please add sqlite.dll to your system path");
        }

        public PersistenceImpl()
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
            if (userDto == null) throw new ArgumentNullException("userDto");

            return _db.Table<DiskDto>().Where(d => d.UserId == userDto.Id).ToList();
        }


        public DiskDto CreateDisk(UserDto userDto, DiskOptionsDto optionsDto)
        {
            if (userDto == null) throw new ArgumentNullException("userDto");
            if (optionsDto == null) throw new ArgumentNullException("optionsDto");

            var disk = new DiskDto { UserId = userDto.Id };
            _db.Insert(disk);

            optionsDto.DiskId = disk.Id;
            _db.Insert(optionsDto);

            return disk;
        }

        public void UpdateDisk(DiskDto diskDto)
        {
            if (diskDto == null) throw new ArgumentNullException("diskDto");

            _db.Update(diskDto);
        }

        public bool RemoveDisk(DiskDto diskDto)
        {
            if (diskDto == null) throw new ArgumentNullException("diskDto");

            if (FindDisk(diskDto) == null) return false;

            _db.Delete<DiskDto>(diskDto.Id);
            return true;
        }

        public DiskDto FindDisk(DiskDto remoteDiskDto)
        {
            if (remoteDiskDto == null) throw new ArgumentNullException("remoteDiskDto");

            return _db.Find<DiskDto>(remoteDiskDto.Id);
        }

        public DiskOptionsDto LoadDiskOptions(int diskId)
        {
            return _db.Find<DiskOptionsDto>(o => o.DiskId == diskId);
        }

        public void SaveDiskOptions(int diskId, DiskOptionsDto optionsDto)
        {
            if (optionsDto == null) throw new ArgumentNullException("optionsDto");

            _db.Delete<DiskOptionsDto>(LoadDiskOptions(diskId).Id);
            optionsDto.DiskId = diskId;
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
            if (userDto == null) throw new ArgumentNullException("userDto");

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

        public DiskDto Disk(DiskDto diskDto)
        {
            if (diskDto == null) throw new ArgumentNullException("diskDto");

            return _db.Find<DiskDto>(diskDto.Id);
        }
    }
}