using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Interfaces;
using VFSBaseTests.Helpers;

namespace VFSBaseTests.Synchronization
{
    [TestClass]
    public class ShiftBlockTest
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
        public void TestBlocksDontChange()
        {
            if (File.Exists(DefaultTestfileFile)) File.Delete(DefaultTestfileFile);
            File.WriteAllText(DefaultTestfileFile, "xxx");

            using (var f = _testHelper.GetFileSystem())
            {
                var root = f.Root; // V0
                var testFolder = f.CreateFolder(root, "test"); // V1

                var blubFolder = f.CreateFolder(testFolder, "blub"); // V2
                f.Import(DefaultTestfileFile, blubFolder, "test.txt", new ImportCallbacks()); // V3
                File.Delete(DefaultTestfileFile);

                testFolder = f.Folders(f.Root).First();
                blubFolder = f.Folders(testFolder).First();
                var testfile = f.Files(blubFolder).First();

                var testFolderBlockNr = testFolder.BlockNumber;
                var blubFolderPredecessorBlockNr = blubFolder.PredecessorBlockNr;
                var blubFolderBlockNr = blubFolder.BlockNumber;
                var testfileBlockNr = testfile.BlockNumber;
                var testfileIndrectNodeNumber = testfile.IndirectNodeNumber;

                const long offset = 10;
                f.ShiftBlocks(1L, 10L);

                testFolder = f.Folders(f.Root).First();
                blubFolder = f.Folders(testFolder).First();
                testfile = f.Files(blubFolder).First();

                var testFolderBlockNrAfter = testFolder.BlockNumber;
                var blubFolderPredecessorBlockNrAfter = blubFolder.PredecessorBlockNr;
                var blubFolderBlockNrAfter = blubFolder.BlockNumber;
                var testfileBlockNrAfter = testfile.BlockNumber;
                var testfileIndrectNodeNumberAfter = testfile.IndirectNodeNumber;

                Assert.AreEqual(testFolderBlockNr + offset, testFolderBlockNrAfter);
                Assert.AreEqual(blubFolderPredecessorBlockNr + offset, blubFolderPredecessorBlockNrAfter);
                Assert.AreEqual(blubFolderBlockNr + offset, blubFolderBlockNrAfter);
                Assert.AreEqual(testfileBlockNr + offset, testfileBlockNrAfter);
                Assert.AreEqual(testfileIndrectNodeNumber + offset, testfileIndrectNodeNumberAfter);
            }
        }

    }
}
