using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSWCFService;
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
            var disk = s.CreateDisk(_user, null);
            Assert.AreEqual(_user, disk.User);
            Assert.AreEqual(1, _persistence.Disks(_user).Count);
        }

        [TestMethod]
        public void TestCreateDiskBad()
        {
            var s = new DiskService { Persistence = _persistence };
            var disk = s.CreateDisk(new User { Login = "xxx", HashedPassword = "yyy" }, null);
            Assert.IsNull(disk);
        }

        [TestMethod]
        public void TestDisks()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var disk = new Disk { User = _user, Uuid = "foo" };
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
            var disk = new Disk { User = _user, Uuid = "foo" };
            _persistence.CreateDisk(_user, disk);
            var s = new DiskService { Persistence = _persistence };
            Assert.IsTrue(s.DeleteDisk(disk));
            Assert.AreEqual(0, s.Disks(_user).Count);
            Assert.IsFalse(s.DeleteDisk(disk));
        }

        [TestMethod]
        public void TestSynchronizationStateUpToDate()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var disk = new Disk { User = _user, Uuid = "foo" };
            _persistence.CreateDisk(_user, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = s.Disks(_user).First();
            Assert.AreEqual(SynchronizationState.UpToDate, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateLocalChanges()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var disk = new Disk { User = _user, Uuid = "foo" };
            _persistence.CreateDisk(_user, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = new Disk { LastServerVersion = disk.LastServerVersion, User = _user, Uuid = disk.Uuid };
            clonedDisk.LocalVersion += 1;
            Assert.AreEqual(SynchronizationState.LocalChanges, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateRemoteChanges()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var disk = new Disk { User = _user, Uuid = "foo", LocalVersion = 10, LastServerVersion = 10};
            _persistence.CreateDisk(_user, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = new Disk { LastServerVersion = 9, LocalVersion = 9, User = _user, Uuid = disk.Uuid };
            Assert.AreEqual(SynchronizationState.RemoteChanges, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateConflicted()
        {
            _persistence.CreateUser(_user.Login, _user.HashedPassword);
            var disk = new Disk { User = _user, Uuid = "foo", LocalVersion = 10, LastServerVersion = 10 };
            _persistence.CreateDisk(_user, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = new Disk{LastServerVersion = 7, LocalVersion = 15, User = _user, Uuid = disk.Uuid};
            Assert.AreEqual(SynchronizationState.Conflicted, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestRegistration()
        {
            var persistence = new Persistence();
            var s = new DiskService { Persistence = persistence };
            var user = s.Register("bla", "blub");
            Assert.AreEqual("bla", user.Login);
            Assert.AreEqual("blub", user.HashedPassword);
            Assert.IsTrue(persistence.UserExists("bla"));
        }

        [TestMethod]
        public void TestRegistrationFail()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "test");
            var s = new DiskService { Persistence = persistence };
            var user = s.Register("bla", "blub");
            Assert.IsNull(user);
        }

        [TestMethod]
        public void TestLogin()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "blub");
            var s = new DiskService { Persistence = persistence };
            var user = s.Login("bla", "blub");
            Assert.AreEqual("bla", user.Login);
            Assert.AreEqual("blub", user.HashedPassword);
        }

        [TestMethod]
        public void TestLoginFail()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "test");
            var s = new DiskService { Persistence = persistence };
            var user = s.Login("bla", "blub");
            Assert.IsNull(user);
        }
    }
}
