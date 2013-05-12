using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSWCFContracts.DataTransferObjects;
using VFSWCFContracts.FaultContracts;
using VFSWCFService.DiskService;
using VFSWCFService.Persistence;

namespace VFSWCFServiceTests
{
    [TestClass]
    public class DiskServiceTest
    {
        private UserDto _userDto;
        private static TestHelper _testHelper;

        [TestInitialize]
        public void InitTestPersistence()
        {
            _testHelper = new TestHelper("../../Testfiles/DiskServiceTest");
            _userDto = new UserDto { Login = "bla", HashedPassword = "blub" };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testHelper.Cleanup();
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestAuthenticationOnLogin()
        {
            using (var s = new DiskServiceImpl(GetPersistence()))
            {
                s.Login("nananana", "blub");
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestAuthenticationOnCreateDisk()
        {
            using (var s = new DiskServiceImpl(GetPersistence()))
            {
                s.Login("nananana", "blub");
                var disk = s.CreateDisk(new UserDto { Login = "bla", HashedPassword = "wrong!" },
                             new DiskOptionsDto { BlockSize = 1000, MasterBlockSize = 1000, SerializedFileSystemOptions = new byte[10] });
            }
        }

        [TestMethod]
        public void TestAutgeneratedIdShouldBeOne()
        {
            using (var persistence = GetPersistence())
            {
                var user = persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                Assert.AreEqual(1, user.Id);
            }
        }

        [TestMethod]
        public void TestCreateDiskSuccess()
        {
            using (var persistence = GetPersistenceWithUser())
            {
                var user = persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var s = new DiskServiceImpl(persistence);
                var disk = s.CreateDisk(user, new DiskOptionsDto());
                Assert.AreEqual(user.Id, disk.UserId);
                Assert.AreEqual(1, persistence.Disks(user).Count);
                Assert.AreEqual(user.Id, persistence.Disks(user).First().UserId);
                Assert.AreEqual(user.Id, disk.UserId);
                Assert.AreEqual(1, disk.UserId);
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestCreateDiskBad()
        {
            using (var persistence = GetPersistence())
            {
                var s = new DiskServiceImpl(persistence);
                var disk = s.CreateDisk(new UserDto { Login = "xxx", HashedPassword = "yyy" }, null);
                Assert.IsNull(disk);
            }
        }

        [TestMethod]
        public void TestDisks()
        {
            using (var persistence = GetPersistenceWithUser())
            {
                var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
                persistence.CreateDisk(_userDto, new DiskOptionsDto());
                var s = new DiskServiceImpl(persistence);
                Assert.AreEqual(1, s.Disks(_userDto).Count);
                Assert.AreEqual(disk.Id, s.Disks(_userDto).First().Id);
            }
        }

        [TestMethod]
        public void TestDisksEmpty()
        {
            using (var persistence = GetPersistenceWithUser())
            {
                var s = new DiskServiceImpl(persistence);
                Assert.AreEqual(0, s.Disks(_userDto).Count);
            }
        }

        [TestMethod]
        public void TestDeleteDisks()
        {
            using (var persistence = GetPersistence())
            {
                var user = persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = persistence.CreateDisk(user, new DiskOptionsDto());
                var s = new DiskServiceImpl(persistence);
                Assert.IsTrue(s.DeleteDisk(user, disk));
                Assert.AreEqual(0, s.Disks(user).Count);
            }
        }

        [TestMethod]
        public void TestSynchronizationStateUpToDate()
        {
            using (var persistence = GetPersistence())
            {
                var user = persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                persistence.CreateDisk(user, new DiskOptionsDto());

                var s = new DiskServiceImpl(persistence);

                var clonedDisk = s.Disks(user).First();
                Assert.AreEqual(SynchronizationState.UpToDate, s.FetchSynchronizationState(user, clonedDisk));
            }
        }

        [TestMethod]
        public void TestSynchronizationStateLocalChanges()
        {
            using (var persistence = GetPersistence())
            {
                var user = persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = persistence.CreateDisk(user, new DiskOptionsDto());

                var s = new DiskServiceImpl(persistence);

                var clonedDisk = new DiskDto { LastServerVersion = disk.LastServerVersion, UserId = user.Id, Id = disk.Id };
                clonedDisk.LocalVersion += 1;
                Assert.AreEqual(SynchronizationState.LocalChanges, s.FetchSynchronizationState(user, clonedDisk));
            }
        }

        [TestMethod]
        public void TestSynchronizationStateRemoteChanges()
        {
            using (var persistence = GetPersistence())
            {
                var user = persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);

                var disk = persistence.CreateDisk(user, new DiskOptionsDto());
                disk.LocalVersion = 10;
                disk.LastServerVersion = 10;
                persistence.UpdateDisk(disk);

                var s = new DiskServiceImpl(persistence);

                var clonedDisk = new DiskDto { LastServerVersion = 9, LocalVersion = 9, UserId = user.Id, Id = disk.Id };
                Assert.AreEqual(SynchronizationState.RemoteChanges, s.FetchSynchronizationState(user, clonedDisk));
            }
        }

        [TestMethod]
        public void TestSynchronizationStateConflicted()
        {
            using (var persistence = GetPersistence())
            {
                var user = persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = persistence.CreateDisk(user, new DiskOptionsDto());
                disk.LocalVersion = 10;
                disk.LastServerVersion = 10;
                persistence.UpdateDisk(disk);

                var s = new DiskServiceImpl(persistence);

                var clonedDisk = new DiskDto { LastServerVersion = 7, LocalVersion = 15, UserId = _userDto.Id, Id = disk.Id };
                Assert.AreEqual(SynchronizationState.Conflicted, s.FetchSynchronizationState(_userDto, clonedDisk));
            }
        }

        [TestMethod]
        public void TestRegistration()
        {
            using (var persistence = GetPersistence())
            {
                var s = new DiskServiceImpl(persistence);
                var user = s.Register("bla", "blub");
                Assert.AreEqual("bla", user.Login);
                Assert.AreEqual("blub", user.HashedPassword);
                Assert.IsFalse(persistence.LoginFree("bla"));
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestRegistrationFail()
        {
            using (var persistence = GetPersistence())
            {
                var u = persistence.CreateUser("bla", "blub");
                var s = new DiskServiceImpl(persistence);
                s.Register(u.Login, "blub");
            }
        }

        [TestMethod]
        public void TestLogin()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser("bla", "blub");
                var s = new DiskServiceImpl(persistence);
                var user = s.Login("bla", "blub");
                Assert.AreEqual("bla", user.Login);
                Assert.AreEqual("blub", user.HashedPassword);
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestLoginFail()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser("bla", "test");
                var s = new DiskServiceImpl(persistence);
                s.Login("bla", "blub");
            }
        }

        private static PersistenceImpl GetPersistence()
        {
            return _testHelper.GetPersistence();
        }

        private PersistenceImpl GetPersistenceWithUser()
        {
            var p = _testHelper.GetPersistence();
            var user = p.CreateUser(_userDto.Login, _userDto.HashedPassword);
            _userDto.Id = user.Id;
            return p;
        }
    }
}
