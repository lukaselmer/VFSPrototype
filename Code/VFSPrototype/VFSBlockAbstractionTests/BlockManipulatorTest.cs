using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBlockAbstraction;
using VFSBlockAbstractionTests.Helpers;

namespace VFSBlockAbstractionTests
{
    [TestClass]
    public class BlockManipulatorTest
    {
        private TestHelper _testHelper;
        private const string DefaultTestfileDirectoryPath = "../../../Testfiles/TestfileBlockManipulatorTest/";
        private string _randomTestFile;

        [TestInitialize]
        public void InitTestHelper ()
        {
            _testHelper = new TestHelper (DefaultTestfileDirectoryPath);
            _randomTestFile = _testHelper.RandomTestfilePath ();
            File.Create(_randomTestFile).Close();
        }

        [TestCleanup]
        public void RemoveTestfile ()
        {
            _testHelper.CleanupTestFolder ();
        }

        [TestMethod]
        [ExpectedException (typeof (ArgumentNullException))]
        public void TestBlockManipulatorSaveConfigException ()
        {
            using (var b = new BlockManipulator(_randomTestFile, 1024, 2048))
            {
                b.SaveConfig(null);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (BlockException))]
        public void TestBlockManipulatorLockBlockException()
        {
            using (var b = new BlockManipulator (_randomTestFile, 1024, 2048)) {
                b.LockBlock(0);
                b.LockBlock(0);
            } 
        }
    }
}
