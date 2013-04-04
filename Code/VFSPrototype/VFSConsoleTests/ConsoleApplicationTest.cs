using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSConsole;

namespace VFSConsoleTests
{
    [TestClass]
    public class ConsoleApplicationTest
    {
        private static FileSystemTextManipulatorMock FileSystemMock()
        {
            return new FileSystemTextManipulatorMock();
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
                Assert.AreEqual(c.Prompt + "kthxbye", mocks.FakeOutLine(true));
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
                Assert.AreEqual(string.Format("{0}Available commands:", c.Prompt), mocks.FakeOutLine(true));
                Assert.AreEqual("cd", mocks.FakeOutLine());
                Assert.AreEqual("delete", mocks.FakeOutLine());
                Assert.AreEqual("exists", mocks.FakeOutLine());
                Assert.AreEqual("exit", mocks.FakeOutLine());
                Assert.AreEqual("help", mocks.FakeOutLine());
                Assert.AreEqual("import", mocks.FakeOutLine());
                Assert.AreEqual("ls", mocks.FakeOutLine());
                Assert.AreEqual("mkdir", mocks.FakeOutLine());
                Assert.AreEqual(c.Prompt + "kthxbye", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestLs()
        {
            var fs = FileSystemMock();
            fs.FolderExists = true;

            fs.CurrentFolders = new List<string> { "Bla", "blurb", "xxx" };

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("ls /");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual(string.Format("{0}Found 3 directories:", c.Prompt), mocks.FakeOutLine(true));
                Assert.AreEqual("Bla", mocks.FakeOutLine());
                Assert.AreEqual("blurb", mocks.FakeOutLine());
                Assert.AreEqual("xxx", mocks.FakeOutLine());
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
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
                Assert.AreEqual(string.Format("{0}File or directory does not exist", c.Prompt), mocks.FakeOutLine(true));
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
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
            fs.IsCurrentDirectory = true;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("mkdir test/blub/bla");
                mocks.FakeInLine("mkdir /test/blub/xxx");
                mocks.FakeInLine("cd /test");
                mocks.FakeInLine("mkdir aaa/bbb");

                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("/> Directory /test/blub/bla created", mocks.FakeOutLine(true));
                Assert.AreEqual("/> Directory /test/blub/xxx created", mocks.FakeOutLine());
                mocks.FakeOutLine(); // skip cd output
                Assert.AreEqual(string.Format("{0}Directory /test/aaa/bbb created", c.Prompt), mocks.FakeOutLine());
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
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
                Assert.AreEqual(string.Format("{0}Yes", c.Prompt), mocks.FakeOutLine(true));
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
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
                Assert.AreEqual(string.Format("{0}No", c.Prompt), mocks.FakeOutLine(true));
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
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
                Assert.AreEqual(string.Format("{0}Deleted /test/blub/bla", c.Prompt), mocks.FakeOutLine(true));
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
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
                Assert.AreEqual(
                    string.Format("{0}Please provide two parameters. E.g. import \"C:\\host system\\path\" /to/dest", c.Prompt),
                    mocks.FakeOutLine(true));
                Assert.AreEqual(
                    string.Format("{0}Please provide two parameters. E.g. import \"C:\\host system\\path\" /to/dest", c.Prompt),
                    mocks.FakeOutLine(true));
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
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
                Assert.AreEqual(string.Format("{0}Imported \"C:\\a\" to \"/bla/a\"", c.Prompt), mocks.FakeOutLine(true));
                Assert.AreEqual(string.Format("{0}Imported \"C:\\test folder\\xxx\" to \"/bla/xxx\"", c.Prompt), mocks.FakeOutLine());
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestCd()
        {
            var fs = FileSystemMock();
            fs.FolderExists = true;
            fs.IsCurrentDirectory = true;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("cd /a/b");
                mocks.FakeInLine("cd /");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("/> Directory changed", mocks.FakeOutLine(true));
                Assert.AreEqual("/a/b> Directory changed", mocks.FakeOutLine());
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestCdInvalidDirecory()
        {
            var fs = FileSystemMock();
            fs.IsCurrentDirectory = false;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("cd /a/b");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("/> Directory /a/b does not exist", mocks.FakeOutLine(true));
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
            }
        }


        [TestMethod]
        public void TestCdRelative()
        {
            var fs = FileSystemMock();
            fs.IsCurrentDirectory = true;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("cd a/b");
                mocks.FakeInLine("cd /");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("/> Directory changed", mocks.FakeOutLine(true));
                Assert.AreEqual("/a/b> Directory changed", mocks.FakeOutLine());
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
            }
        }


        [TestMethod]
        public void TestCdRemoveSlashAtEnd()
        {
            var fs = FileSystemMock();
            fs.IsCurrentDirectory = true;

            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("cd /a/b/");
                mocks.FakeInLine("cd /");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(new ConsoleApplicationSettings(mocks.In, mocks.Out), fs);
                c.Run();
                Assert.AreEqual("/> Directory changed", mocks.FakeOutLine(true));
                Assert.AreEqual("/a/b> Directory changed", mocks.FakeOutLine());
                Assert.AreEqual(string.Format("{0}kthxbye", c.Prompt), mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestIOOfConfigFile()
        {
            var c = new ConsoleIOConsoleApplicationSettings();
            Assert.AreSame(c.Reader, Console.In);
            Assert.AreSame(c.Writer, Console.Out);
        }
    }
}
