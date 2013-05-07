using System.Linq;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSWCFContracts.Contracts;
using VFSWCFService.DataTransferObjects;
using VFSWCFService.DiskService;
using VFSWCFService.Persistence;

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

        [ExpectedException(typeof(FaultException<ServiceException>))]
        [TestMethod]
        public void TestAuthenticationOnLogin()
        {
            var s = new DiskService(_persistence);
            s.Login("nananana", "blub");
        }

        [ExpectedException(typeof(FaultException<ServiceException>))]
        [TestMethod]
        public void TestAuthenticationOnCreateDisk()
        {
            var s = new DiskService(_persistence);
            s.Login("nananana", "blub");
            s.CreateDisk(new UserDto {Login = "bla", HashedPassword = "wrong!"},
                         new DiskOptionsDto {BlockSize = 1000, MasterBlockSize = 1000, SerializedFileSystemOptions = new byte[10]});
        }

        [TestMethod]
        public void TestAutgeneratedIdShouldBeOne()
        {
            _persistence.Clear();
            var user = _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            Assert.AreEqual(1, user.Id);
        }

        [TestMethod]
        public void TestCreateDiskSuccess()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var s = new DiskService(_persistence);
            var disk = s.CreateDisk(_userDto, null);
            Assert.AreEqual(_userDto.Id, disk.UserId);
            Assert.AreEqual(1, _persistence.Disks(_userDto).Count);
        }

        [ExpectedException(typeof(FaultException<ServiceException>))]
        [TestMethod]
        public void TestCreateDiskBad()
        {
            _persistence.Clear();
            var s = new DiskService(_persistence);
            var disk = s.CreateDisk(new UserDto { Login = "xxx", HashedPassword = "yyy" }, null);
            Assert.IsNull(disk);
        }

        [TestMethod]
        public void TestDisks()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
            _persistence.CreateDisk(_userDto, disk);
            var s = new DiskService(_persistence);
            Assert.AreEqual(1, s.Disks(_userDto).Count);
            Assert.AreEqual(disk.Id, s.Disks(_userDto).First().Id);
        }

        [TestMethod]
        public void TestDisksEmpty()
        {
            _persistence.Clear();
            _userDto = _persistence.CreateUser("bla", "blub");
            var s = new DiskService(_persistence);
            Assert.AreEqual(0, s.Disks(_userDto).Count);
        }

        [TestMethod]
        public void TestDeleteDisks()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
            _persistence.CreateDisk(_userDto, disk);
            var s = new DiskService(_persistence);
            Assert.IsTrue(s.DeleteDisk(_userDto, disk));
            Assert.AreEqual(0, s.Disks(_userDto).Count);
            Assert.IsFalse(s.DeleteDisk(_userDto, disk));
        }

        [TestMethod]
        public void TestSynchronizationStateUpToDate()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService(_persistence);

            var clonedDisk = s.Disks(_userDto).First();
            Assert.AreEqual(SynchronizationState.UpToDate, s.FetchSynchronizationState(_userDto, clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateLocalChanges()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService(_persistence);

            var clonedDisk = new DiskDto { LastServerVersion = disk.LastServerVersion, UserId = _userDto.Id, Id = disk.Id };
            clonedDisk.LocalVersion += 1;
            Assert.AreEqual(SynchronizationState.LocalChanges, s.FetchSynchronizationState(_userDto, clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateRemoteChanges()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserId = _userDto.Id, Id = 1, LocalVersion = 10, LastServerVersion = 10 };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService(_persistence);

            var clonedDisk = new DiskDto { LastServerVersion = 9, LocalVersion = 9, UserId = _userDto.Id, Id = disk.Id };
            Assert.AreEqual(SynchronizationState.RemoteChanges, s.FetchSynchronizationState(_userDto, clonedDisk));
        }

        [TestMethod]
        public void TestSynchronizationStateConflicted()
        {
            _persistence.Clear();
            _persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
            var disk = new DiskDto { UserId = _userDto.Id, Id = 1, LocalVersion = 10, LastServerVersion = 10 };
            _persistence.CreateDisk(_userDto, disk);

            var s = new DiskService(_persistence);

            var clonedDisk = new DiskDto { LastServerVersion = 7, LocalVersion = 15, UserId = _userDto.Id, Id = disk.Id };
            Assert.AreEqual(SynchronizationState.Conflicted, s.FetchSynchronizationState(_userDto, clonedDisk));
        }

        [TestMethod]
        public void TestRegistration()
        {
            var persistence = new Persistence();
            var s = new DiskService(persistence);
            var user = s.Register("bla", "blub");
            Assert.AreEqual("bla", user.Login);
            Assert.AreEqual("blub", user.HashedPassword);
            Assert.IsFalse(persistence.LoginFree("bla"));
        }

        [TestMethod]
        public void TestRegistrationFail()
        {
            var persistence = new Persistence();
            var u = persistence.CreateUser("bla", "test");
            var s = new DiskService(persistence);
            var user = s.Register("bla", "blub");
            Assert.IsNull(user);
        }

        [TestMethod]
        public void TestLogin()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "blub");
            var s = new DiskService(persistence);
            var user = s.Login("bla", "blub");
            Assert.AreEqual("bla", user.Login);
            Assert.AreEqual("blub", user.HashedPassword);
        }

        [ExpectedException(typeof(FaultException<ServiceException>))]
        [TestMethod]
        public void TestLoginFail()
        {
            var persistence = new Persistence();
            persistence.CreateUser("bla", "test");
            var s = new DiskService(persistence);
            s.Login("bla", "blub");
        }
    }
}
