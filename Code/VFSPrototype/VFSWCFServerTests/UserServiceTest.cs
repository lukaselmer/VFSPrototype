using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSWCFService;
using VFSWCFService.Common;
using VFSWCFService.UserService;

namespace VFSWCFServiceTests
{
    [TestClass]
    public class UserServiceTest
    {
        [TestMethod]
        public void TestRegistration()
        {
            var persistence = new Persistence();
            var s = new UserService { Persistence = persistence };
            var user = s.Register("bla", "blub");
            Assert.AreEqual("bla", user.Login);
            Assert.AreEqual("blub", user.HashedPassword);
            Assert.IsTrue(persistence.Exists("bla"));
        }

        [TestMethod]
        public void TestRegistrationFail()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "test");
            var s = new UserService { Persistence = persistence };
            var user = s.Register("bla", "blub");
            Assert.IsNull(user);
        }

        [TestMethod]
        public void TestLogin()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "blub");
            var s = new UserService { Persistence = persistence };
            var user = s.Login("bla", "blub");
            Assert.AreEqual("bla", user.Login);
            Assert.AreEqual("blub", user.HashedPassword);
        }

        [TestMethod]
        public void TestLoginFail()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "test");
            var s = new UserService { Persistence = persistence };
            var user = s.Login("bla", "blub");
            Assert.IsNull(user);
        }
    }
}
