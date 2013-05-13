using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSWCFContracts.DataTransferObjects;
using VFSWCFContracts.FaultContracts;
using VFSWCFService.DiskService;
using VFSWCFService.Persistence;

namespace VFSWCFServiceTests
{
    [TestClass]
    public class DiskReadWriteTest
    {
        private UserDto _userDto;
        private static TestHelper _testHelper;
        private readonly DiskOptionsDto _diskOptions = new DiskOptionsDto { BlockSize = 100, MasterBlockSize = 1000, SerializedFileSystemOptions = new byte[250] };
        private DiskDto _disk;

        [TestInitialize]
        public void InitTestPersistence()
        {
            _testHelper = new TestHelper("../../Testfiles/DiskReadWriteTest");
            _userDto = new UserDto { Login = "bla", HashedPassword = "blub" };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _testHelper.Cleanup();
        }

        [TestMethod]
        public void TestWriteRead()
        {
            using (var s = new DiskServiceImpl(GetPersistenceWithData()))
            {
                var expected = new byte[100];
                new Random(1).NextBytes(expected);

                s.WriteBlock(_userDto, _disk.Id, 33, expected);

                var actual = s.ReadBlock(_userDto, _disk.Id, 33);

                for (var i = 0; i < expected.Length; i++) Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestWriteFail()
        {
            using (var s = new DiskServiceImpl(GetPersistenceWithData()))
            {
                s.WriteBlock(new UserDto { Login = "bla", HashedPassword = "Wrong!" }, _disk.Id, 33, new byte[33]);
            }
        }

        [ExpectedException(typeof(FaultException<ServiceFault>))]
        [TestMethod]
        public void TestReadFail()
        {
            using (var s = new DiskServiceImpl(GetPersistenceWithData()))
            {
                s.ReadBlock(new UserDto { Login = "bla", HashedPassword = "Wrong!" }, _disk.Id, 33);
            }
        }

        private PersistenceImpl GetPersistenceWithData()
        {
            var p = _testHelper.GetPersistence();

            var user = p.CreateUser(_userDto.Login, _userDto.HashedPassword);
            _userDto.Id = user.Id;

            _disk = p.CreateDisk(_userDto, _diskOptions);

            return p;
        }
    }
}
