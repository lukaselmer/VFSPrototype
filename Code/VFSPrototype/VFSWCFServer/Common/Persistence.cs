using System.Collections.Generic;
using VFSWCFService.UserService;

namespace VFSWCFService.Common
{
    /// <summary>
    /// The persistence is responsible for the data persistence.
    /// </summary>
    public class Persistence
    {
        private readonly Dictionary<string, User> _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="Persistence"/> class.
        /// </summary>
        public Persistence()
        {
            _storage = new Dictionary<string, User>();
        }

        /// <summary>
        /// Checks if the user with the specified login exists.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        public bool Exists(string login)
        {
            return _storage.ContainsKey(login);
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
            _storage[login] = u;
            return u;
        }

        /// <summary>
        /// Finds the user.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        internal User FindUser(string login)
        {
            return _storage[login];
        }
    }
}