using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.DiskServiceReference;
using VFSBase.Exceptions;
using VFSBase.Factories;
using VFSBase.Implementation;
using VFSBase.Synchronization;
using VFSBaseTests.Helpers;

namespace VFSBaseTests.Factories
{
    [TestClass]
    public class FileSystemTestManipulatorFactoryTest
    {
        private TestHelper _helper;
        private const string DefaultTestfileFolder = "../../../Testfiles/FileSystemTestManipulatorFactoryTest/";

        [TestInitialize]
        public void InitHelper()
        {
            _helper = new TestHelper(DefaultTestfileFolder);
        }

        [TestCleanup]
        public void CleanupHelper()
        {
            _helper.CleanupTestFolder();
        }

        [ExpectedException(typeof(VFSException))]
        [TestMethod]
        public void TestCreateFail()
        {
            FileSystemOptions options;
            using (var fs = _helper.GetFileSystem())
            {
                options = fs.FileSystemOptions;
            }

            var factory = new FileSystemTextManipulatorFactory();
            using (var m = factory.Create(options, ""))
            {
                Assert.IsNotNull(m);
            }
        }

        [TestMethod]
        public void TestCreate()
        {
            FileSystemOptions options;
            using (var fs = _helper.GetFileSystem())
            {
                options = fs.FileSystemOptions;
            }

            File.Delete(options.Location);

            var factory = new FileSystemTextManipulatorFactory();
            using (var m = factory.Create(options, ""))
            {
                Assert.IsNotNull(m);
            }
        }

        [TestMethod]
        public void TestOpen()
        {
            FileSystemOptions options;
            using (var fs = _helper.GetFileSystem())
            {
                options = fs.FileSystemOptions;
            }

            var factory = new FileSystemTextManipulatorFactory();
            using (var m = factory.Open(options.Location, ""))
            {
                Assert.IsNotNull(m);
            }
        }

        [TestMethod]
        public void TestLink()
        {
            FileSystemOptions options;
            using (var fs = _helper.GetFileSystem())
            {
                options = fs.FileSystemOptions;
            }

            var diskOptions = SynchronizationHelper.CalculateDiskOptions(options);

            var factory = new FileSystemTextManipulatorFactory();
            factory.Link(diskOptions, options.Location);
        }
    }
}
