using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Callbacks;
using VFSBase.DiskServiceReference;
using VFSBase.Exceptions;
using VFSBase.Synchronization;
using VFSBaseTests.Helpers;
using VFSBaseTests.Mocks;

namespace VFSBaseTests.Synchronization
{
    [TestClass]
    public class SynchronizationTest
    {
        private TestHelper _testHelper;
        private const string DefaultTestfileFolder = "../../../Testfiles/ShiftBlockTest/";
        private const string DefaultTestfileFile = "../../../Testfiles/ShiftBlockTest/test.txt";


        [TestInitialize]
        public void CreateTestFolder()
        {
            _testHelper = new TestHelper(DefaultTestfileFolder);
        }

        [TestCleanup]
        public void RemoveTestfile()
        {
            _testHelper.CleanupTestFolder();
        }

        [TestMethod]
        public void TestFetchRemoteChanges()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                Action finished = () => { };
                Action<long, long> progrssChanged = (i, j) => { };

                f.FileSystemOptions.Id = 33;

                var m = new DiskServiceMock();
                SynchronizationService s = new SynchronizationServiceMock(f, new UserDto(), new SynchronizationCallbacks(finished, progrssChanged), m);

                m.SynchronizationState = SynchronizationState.LocalChanges;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());

                m.SynchronizationState = SynchronizationState.RemoteChanges;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());

                m.SynchronizationState = SynchronizationState.UpToDate;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());

                m.SynchronizationState = SynchronizationState.Conflicted;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());
            }
        }

        [TestMethod]
        public void TestFetchRemoteChangesWithCreate()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                Action finished = () => { };
                Action<long, long> progrssChanged = (i, j) => { };

                var m = new DiskServiceMock { DiskFake = new DiskDto { Id = 33 } };
                SynchronizationService s = new SynchronizationServiceMock(f, new UserDto(), new SynchronizationCallbacks(finished, progrssChanged), m);

                m.SynchronizationState = SynchronizationState.LocalChanges;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());

                m.SynchronizationState = SynchronizationState.RemoteChanges;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());

                m.SynchronizationState = SynchronizationState.UpToDate;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());

                m.SynchronizationState = SynchronizationState.Conflicted;
                Assert.AreEqual(m.SynchronizationState, s.FetchSynchronizationState());
            }
        }

        [TestMethod]
        public void TestSynchronizeWhenUpToDate()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                Action finished = () => { };
                Action<long, long> progrssChanged = (i, j) => { };

                var m = new DiskServiceMock
                {
                    DiskFake = new DiskDto { Id = 33 },
                    DiskOptionsMock = new DiskOptionsDto
                    {
                        SerializedFileSystemOptions = SynchronizationHelper.CalculateDiskOptions(f.FileSystemOptions).SerializedFileSystemOptions
                    }
                };
                SynchronizationService s = new SynchronizationServiceMock(f, new UserDto(), new SynchronizationCallbacks(finished, progrssChanged), m);

                s.Synchronize();
            }
        }

        [ExpectedException(typeof(VFSException))]
        [TestMethod]
        public void TestSynchronizeWhenConflicted()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                Action finished = () => { };
                Action<long, long> progrssChanged = (i, j) => { };

                var m = new DiskServiceMock
                {
                    DiskFake = new DiskDto { Id = 33 },
                    SynchronizationState = SynchronizationState.Conflicted,
                    DiskOptionsMock = new DiskOptionsDto
                    {
                        SerializedFileSystemOptions = SynchronizationHelper.CalculateDiskOptions(f.FileSystemOptions).SerializedFileSystemOptions
                    }
                };
                SynchronizationService s = new SynchronizationServiceMock(f, new UserDto(), new SynchronizationCallbacks(finished, progrssChanged), m);

                s.Synchronize();
            }
        }

        [TestMethod]
        public void TestSynchronizeWhenLocalChanges()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                Action finished = () => { };
                Action<long, long> progrssChanged = (i, j) => { };

                var m = new DiskServiceMock
                {
                    DiskFake = new DiskDto { Id = 33 },
                    SynchronizationState = SynchronizationState.LocalChanges,
                    DiskOptionsMock = new DiskOptionsDto
                    {
                        SerializedFileSystemOptions = SynchronizationHelper.CalculateDiskOptions(f.FileSystemOptions).SerializedFileSystemOptions
                    }
                };
                SynchronizationService s = new SynchronizationServiceMock(f, new UserDto(), new SynchronizationCallbacks(finished, progrssChanged), m);

                Assert.IsNull(m.DiskOptionsResult);

                s.Synchronize();

                Assert.IsNotNull(m.DiskOptionsResult);
            }
        }

        [TestMethod]
        public void TestSynchronizeWhenRemoteChanges()
        {
            using (var f = _testHelper.GetFileSystem())
            {
                Action finished = () => { };
                Action<long, long> progrssChanged = (i, j) => { };

                var m = new DiskServiceMock
                {
                    DiskFake = new DiskDto { Id = 33 },
                    SynchronizationState = SynchronizationState.RemoteChanges,
                    DiskOptionsMock = new DiskOptionsDto
                    {
                        SerializedFileSystemOptions = SynchronizationHelper.CalculateDiskOptions(f.FileSystemOptions).SerializedFileSystemOptions
                    }
                };
                SynchronizationService s = new SynchronizationServiceMock(f, new UserDto(), new SynchronizationCallbacks(finished, progrssChanged), m);

                Assert.IsNull(m.DiskOptionsResult);

                s.Synchronize();
            }
        }

    }
}
