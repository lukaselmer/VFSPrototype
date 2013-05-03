using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSWCFService;
using VFSWCFService.Common;
using VFSWCFService.DiskService;
using VFSWCFService.UserService;

namespace VFSWCFServiceTests
{
    [TestClass]
    public class DiskServiceTest
    {
        private Persistence _persistence;
        private User _user;

        [TestInitialize]
        public void InitTestPersistence()
        {
            _persistence = new Persistence();
            _user = _persistence.CreateUser("bla", "blub");
        }

        [TestMethod]
        public void TestCreateDiskSuccess()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var s = new DiskService { Persistence = _persistence };
            var disk = s.CreateDisk(_user);
            Assert.AreEqual(_user, disk.User);
            Assert.AreEqual(1, _persistence.Disks(_user).Count);
        }

        [TestMethod]
        public void TestCreateDiskBad()
        {
            var s = new DiskService { Persistence = _persistence };
            var disk = s.CreateDisk(new User { Login = "xxx", HashedPassword = "yyy" });
            Assert.IsNull(disk);
        }

        [TestMethod]
        public void TestDisks()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var disk = new Disk { User = _user };
            _persistence.CreateDisk(_user, disk);
            var s = new DiskService { Persistence = _persistence };
            Assert.AreEqual(1, s.Disks(_user).Count);
            Assert.AreEqual(disk, s.Disks(_user).First());
        }

        [TestMethod]
        public void TestDisksEmpty()
        {
            var s = new DiskService { Persistence = _persistence };
            Assert.AreEqual(0, s.Disks(_user).Count);
        }

        [TestMethod]
        public void TestDeleteDisks()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var disk = new Disk { User = _user };
            _persistence.CreateDisk(_user, disk);
            var s = new DiskService { Persistence = _persistence };
            Assert.IsTrue(s.DeleteDisk(disk));
            Assert.AreEqual(0, s.Disks(_user).Count);
            Assert.IsFalse(s.DeleteDisk(disk));
        }
    }
}
