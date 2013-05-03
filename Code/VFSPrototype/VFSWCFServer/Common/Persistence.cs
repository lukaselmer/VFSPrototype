using System.Collections.Generic;
using VFSWCFService.DiskService;
using VFSWCFService.UserService;

namespace VFSWCFService.Common
{
    /// <summary>
    /// The persistence is responsible for the data persistence.
    /// </summary>
    public class Persistence
    {
        private readonly Dictionary<string, User> _userStorage;
        private readonly Dictionary<string, IList<Disk>> _diskStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="Persistence"/> class.
        /// </summary>
        public Persistence()
        {
            _userStorage = new Dictionary<string, User>();
            _diskStorage = new Dictionary<string, IList<Disk>>();
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
            return _diskStorage.ContainsKey(user.Login) ? _diskStorage[user.Login] : null;
        }

        public void CreateDisk(User user, Disk disk)
        {
            if (!_diskStorage.ContainsKey(user.Login)) _diskStorage[user.Login] = new List<Disk>();
            _diskStorage[user.Login].Add(disk);
        }
    }
}