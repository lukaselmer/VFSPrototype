﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase;
using VFSConsole;

namespace VFSConsoleTests
{
    [TestClass]
    public class ConsoleApplicationTest
    {
        private static FileSystemManipulatorMock FileSystemMock()
        {
            return new FileSystemManipulatorMock();
        }

        [TestMethod]
        public void TestExit()
        {
            var fs = FileSystemMock();
            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine(true));
            }
        }

        [TestMethod]
        public void TestHelp()
        {
            var fs = FileSystemMock();
            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("help");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> Available commands:", mocks.FakeOutLine(true));
                Assert.AreEqual("delete", mocks.FakeOutLine());
                Assert.AreEqual("exists", mocks.FakeOutLine());
                Assert.AreEqual("exit", mocks.FakeOutLine());
                Assert.AreEqual("help", mocks.FakeOutLine());
                Assert.AreEqual("import", mocks.FakeOutLine());
                Assert.AreEqual("ls", mocks.FakeOutLine());
                Assert.AreEqual("mkdir", mocks.FakeOutLine());
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestLs()
        {
            var fs = FileSystemMock();
            fs.FolderExists = true;

            fs._folders = new SortedSet<string> {"Bla", "blurb", "xxx"};

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("ls /");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> Found 3 directories:", mocks.FakeOutLine(true));
                Assert.AreEqual("Bla", mocks.FakeOutLine());
                Assert.AreEqual("blurb", mocks.FakeOutLine());
                Assert.AreEqual("xxx", mocks.FakeOutLine());
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestLsNotExisting()
        {
            var fs = FileSystemMock();
            fs.FolderExists = false;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("ls test");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> File or directory does not exist", mocks.FakeOutLine(true));
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void FileSystemMustNotBeNullConstructorTest()
        {
            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("exit", true);
                new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), null);
            }
        }

        [TestMethod]
        public void TestMkdir()
        {
            var fs = FileSystemMock();
            fs.FolderExists = true;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("mkdir test/blub/bla");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> Directory test/blub/bla created", mocks.FakeOutLine(true));
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestExistsYes()
        {
            var fs = FileSystemMock();
            fs.FolderExists = true;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("exists test/blub/bla");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> Yes", mocks.FakeOutLine(true));
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestExistsNo()
        {
            var fs = FileSystemMock();
            fs.FolderExists = false;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("exists test/blub/bla");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> No", mocks.FakeOutLine(true));
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestDeleted()
        {
            var fs = FileSystemMock();
            fs.FolderExists = false;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("delete test/blub/bla");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("> Deleted test/blub/bla", mocks.FakeOutLine(true));
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestImportWrongParameters()
        {
            var fs = FileSystemMock();
            fs.FolderExists = false;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("import blub");
                mocks.FakeInLine(@"import bl""ub");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual(@"> Please provide two parameters. E.g. import ""C:\host system\path"" /to/dest", mocks.FakeOutLine(true));
                Assert.AreEqual(@"> Please provide two parameters. E.g. import ""C:\host system\path"" /to/dest", mocks.FakeOutLine(true));
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestImport()
        {
            var fs = FileSystemMock();
            fs.FolderExists = false;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine(@"import C:\a /bla/a");
                mocks.FakeInLine(@"import ""C:\test folder\xxx"" /bla/xxx");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual(@"> Imported ""C:\a"" to ""/bla/a""", mocks.FakeOutLine(true));
                Assert.AreEqual(@"> Imported ""C:\test folder\xxx"" to ""/bla/xxx""", mocks.FakeOutLine());
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine());
            }
        }
    }
}