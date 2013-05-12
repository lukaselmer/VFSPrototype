using System;
using System.IO;
using VFSWCFService.Persistence;

namespace VFSWCFServiceTests
{
    public class TestHelper
    {
        private readonly string _testdirectoryPath;

        public TestHelper(string testdirectoryPath)
        {
            _testdirectoryPath = testdirectoryPath;

            if (!Directory.Exists(_testdirectoryPath)) Directory.CreateDirectory(_testdirectoryPath);
        }

        public PersistenceImpl GetPersistence()
        {
            return new PersistenceImpl(Path.Combine(_testdirectoryPath, Guid.NewGuid() + ".sqlite"));
        }

        public void Cleanup()
        {
            if (Directory.Exists(_testdirectoryPath)) Directory.Delete(_testdirectoryPath, true);
        }
    }
}