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
            _testHelper = new TestHelper("../../Testfiles/TmpDatabases");
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
            using (var s = new DiskService(GetPersistence()))
            {
                s.Login("nananana", "blub");
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestAuthenticationOnCreateDisk()
        {
            using (var s = new DiskService(GetPersistence()))
            {
                s.Login("nananana", "blub");
                s.CreateDisk(new UserDto { Login = "bla", HashedPassword = "wrong!" },
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
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var s = new DiskService(persistence);
                var disk = s.CreateDisk(_userDto, null);
                Assert.AreEqual(_userDto.Id, disk.UserId);
                Assert.AreEqual(1, persistence.Disks(_userDto).Count);
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestCreateDiskBad()
        {
            using (var persistence = GetPersistence())
            {
                var s = new DiskService(persistence);
                var disk = s.CreateDisk(new UserDto { Login = "xxx", HashedPassword = "yyy" }, null);
                Assert.IsNull(disk);
            }
        }

        [TestMethod]
        public void TestDisks()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
                persistence.CreateDisk(_userDto, disk);
                var s = new DiskService(persistence);
                Assert.AreEqual(1, s.Disks(_userDto).Count);
                Assert.AreEqual(disk.Id, s.Disks(_userDto).First().Id);
            }
        }

        [TestMethod]
        public void TestDisksEmpty()
        {
            using (var persistence = GetPersistence())
            {
                _userDto = persistence.CreateUser("bla", "blub");
                var s = new DiskService(persistence);
                Assert.AreEqual(0, s.Disks(_userDto).Count);
            }
        }

        [TestMethod]
        public void TestDeleteDisks()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
                persistence.CreateDisk(_userDto, disk);
                var s = new DiskService(persistence);
                Assert.IsTrue(s.DeleteDisk(_userDto, disk));
                Assert.AreEqual(0, s.Disks(_userDto).Count);
                Assert.IsFalse(s.DeleteDisk(_userDto, disk));
            }
        }

        [TestMethod]
        public void TestSynchronizationStateUpToDate()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
                persistence.CreateDisk(_userDto, disk);

                var s = new DiskService(persistence);

                var clonedDisk = s.Disks(_userDto).First();
                Assert.AreEqual(SynchronizationState.UpToDate, s.FetchSynchronizationState(_userDto, clonedDisk));
            }
        }

        [TestMethod]
        public void TestSynchronizationStateLocalChanges()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = new DiskDto { UserId = _userDto.Id, Id = 1 };
                persistence.CreateDisk(_userDto, disk);

                var s = new DiskService(persistence);

                var clonedDisk = new DiskDto { LastServerVersion = disk.LastServerVersion, UserId = _userDto.Id, Id = disk.Id };
                clonedDisk.LocalVersion += 1;
                Assert.AreEqual(SynchronizationState.LocalChanges, s.FetchSynchronizationState(_userDto, clonedDisk));
            }
        }

        [TestMethod]
        public void TestSynchronizationStateRemoteChanges()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = new DiskDto { UserId = _userDto.Id, Id = 1, LocalVersion = 10, LastServerVersion = 10 };
                persistence.CreateDisk(_userDto, disk);

                var s = new DiskService(persistence);

                var clonedDisk = new DiskDto { LastServerVersion = 9, LocalVersion = 9, UserId = _userDto.Id, Id = disk.Id };
                Assert.AreEqual(SynchronizationState.RemoteChanges, s.FetchSynchronizationState(_userDto, clonedDisk));
            }
        }

        [TestMethod]
        public void TestSynchronizationStateConflicted()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser(_userDto.Login, _userDto.HashedPassword);
                var disk = new DiskDto { UserId = _userDto.Id, Id = 1, LocalVersion = 10, LastServerVersion = 10 };
                persistence.CreateDisk(_userDto, disk);

                var s = new DiskService(persistence);

                var clonedDisk = new DiskDto { LastServerVersion = 7, LocalVersion = 15, UserId = _userDto.Id, Id = disk.Id };
                Assert.AreEqual(SynchronizationState.Conflicted, s.FetchSynchronizationState(_userDto, clonedDisk));
            }
        }

        [TestMethod]
        public void TestRegistration()
        {
            using (var persistence = GetPersistence())
            {
                var s = new DiskService(persistence);
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
                var s = new DiskService(persistence);
                s.Register(u.Login, "blub");
            }
        }

        [TestMethod]
        public void TestLogin()
        {
            using (var persistence = GetPersistence())
            {
                persistence.CreateUser("bla", "blub");
                var s = new DiskService(persistence);
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
                var s = new DiskService(persistence);
                s.Login("bla", "blub");
            }
        }

        private static Persistence GetPersistence()
        {
            return _testHelper.GetPersistence();
        }
    }
}
