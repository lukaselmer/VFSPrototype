using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBaseTests.Helpers;

namespace VFSBaseTests.History
{
    [TestClass]
    public class BlocksDontChangeTest
    {
        private TestHelper _testHelper;
        private const string DefaultTestfileFolder = "../../../Testfiles/BlocksDontChangeTests/";

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
        public void TestBlocksDontChange()
        {
            using (var m = _testHelper.GetManipulator())
            {
                //TODO: implement this
            }
        }

    }
}
