using VFSWCFService.Common;

namespace VFSWCFService.UserService
{
    /// <summary>
    /// The user service handles registration and login
    /// </summary>
    public class UserService : IUserService
    {
        internal Persistence Persistence { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService" /> class.
        /// </summary>
        public UserService()
        {
            Persistence = new Persistence();
        }

        /// <summary>
        /// Registers a user with the specified login and password.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>If successful, the user, null otherwise.</returns>
        public User Register(string login, string hashedPassword)
        {
            return Persistence.UserExists(login) ? null : Persistence.CreateUser(login, hashedPassword);
        }

        /// <summary>
        /// Authenticates the user with the login and password.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>The user if login is successful, null otherwise.</returns>
        public User Login(string login, string hashedPassword)
        {
            if (!Persistence.UserExists(login)) return null;

            var u = Persistence.FindUser(login);
            return u.HashedPassword == hashedPassword ? u : null;
        }
    }
}