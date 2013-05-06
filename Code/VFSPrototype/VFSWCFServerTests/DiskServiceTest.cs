using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSWCFService;
using VFSWCFService.DiskService;

namespace VFSWCFServiceTests
{
    [TestClass]
    public class DiskServiceTest
    {
        private Persistence _persistence;
        private UserDto _userDto;

        [TestInitialize]
        public void InitTestPersistence()
        {
            _persistence = new Persistence();
            _persistence.Clear();
            _userDto = _persistence.CreateUser("bla", "blub");
        }

        [TestMethod]
        public void TestCreateDiskSuccess()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var s = new DiskService { Persistence = _persistence };
            var disk = s.CreateDisk(_userDto, null);
            Assert.AreEqual(_userDto.Login, disk.UserLogin);
            Assert.AreEqual(1, _persistence.Disks(_userDto.Login).Count);
        }

        [TestMethod]
        public void TestCreateDiskBad()
        {
            _persistence.Clear();
            var s = new DiskService { Persistence = _persistence };
            var disk = s.CreateDisk(new UserDto { Login = "xxx", HashedPassword = "yyy" }, null);
            Assert.IsNull(disk);
        }

        [TestMethod]
        public void TestDisks()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserLogin = _userDto.Login, Uuid = "foo" };
            _persistence.CreateDisk(_userDto, disk);
            var s = new DiskService { Persistence = _persistence };
            Assert.AreEqual(1, s.Disks(_userDto).Count);
            Assert.AreEqual(disk.Uuid, s.Disks(_userDto).First().Uuid);
        }

        [TestMethod]
        public void TestDisksEmpty()
        {
            _persistence.Clear();
            var s = new DiskService { Persistence = _persistence };
            Assert.AreEqual(0, s.Disks(_userDto).Count);
        }

        [TestMethod]
        public void TestDeleteDisks()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserLogin = _userDto.Login, Uuid = "foo" };
            _persistence.CreateDisk(_userDto, disk);
            var s = new DiskService { Persistence = _persistence };
            Assert.IsTrue(s.DeleteDisk(disk));
            Assert.AreEqual(0, s.Disks(_userDto).Count);
            Assert.IsFalse(s.DeleteDisk(disk));
        }

        [TestMethod]
        public void TestSynchronizationStateUpToDate()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserLogin = _userDto.Login, Uuid = "foo" };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = s.Disks(_userDto).First();
            Assert.AreEqual(SynchronizationState.UpToDate, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateLocalChanges()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserLogin = _userDto.Login, Uuid = "foo" };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = new DiskDto { LastServerVersion = disk.LastServerVersion, UserLogin = _userDto.Login, Uuid = disk.Uuid };
            clonedDisk.LocalVersion += 1;
            Assert.AreEqual(SynchronizationState.LocalChanges, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateRemoteChanges()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserLogin = _userDto.Login, Uuid = "foo", LocalVersion = 10, LastServerVersion = 10 };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = new DiskDto { LastServerVersion = 9, LocalVersion = 9, UserLogin = _userDto.Login, Uuid = disk.Uuid };
            Assert.AreEqual(SynchronizationState.RemoteChanges, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateConflicted()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserLogin = _userDto.Login, Uuid = "foo", LocalVersion = 10, LastServerVersion = 10 };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService { Persistence = _persistence };

            var clonedDisk = new DiskDto { LastServerVersion = 7, LocalVersion = 15, UserLogin = _userDto.Login, Uuid = disk.Uuid };
            Assert.AreEqual(SynchronizationState.Conflicted, s.FetchSynchronizationState(clonedDisk));
        }

        [TestMethod]
        public void TestRegistration()
        {
            _persistence.Clear();
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
            _persistence.Clear();
            var persistence = new Persistence();
            persistence.CreateUser("bla", "test");
            var s = new DiskService { Persistence = persistence };
            var user = s.Register("bla", "blub");
            Assert.IsNull(user);
        }

        [TestMethod]
        public void TestLogin()
        {
            _persistence.Clear();
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
            _persistence.Clear();
            var persistence = new Persistence();
            persistence.CreateUser("bla", "test");
            var s = new DiskService { Persistence = persistence };
            var user = s.Login("bla", "blub");
            Assert.IsNull(user);
        }
    }
}
