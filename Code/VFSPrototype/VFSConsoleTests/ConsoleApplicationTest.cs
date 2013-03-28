using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSConsole;

namespace VFSConsoleTests
{
    [TestClass]
    public class ConsoleApplicationTest
    {
        [TestMethod]
        public void TestExit()
        {
            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(mocks.In, mocks.Out);
                c.Run();
                Assert.AreEqual("> kthxbye", mocks.FakeOutLine(true));
            }
        }

        [TestMethod]
        public void TestHelp()
        {
            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("help");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(mocks.In, mocks.Out);
                c.Run();
                Assert.AreEqual("> Available commands:", mocks.FakeOutLine(true));
                Assert.AreEqual("help", mocks.FakeOutLine());
                Assert.AreEqual("ls", mocks.FakeOutLine());
                Assert.AreEqual("exit", mocks.FakeOutLine());
            }
        }

        [TestMethod]
        public void TestLs()
        {
            using (var mocks = new InOutMocks())
            {
                mocks.FakeInLine("ls");
                mocks.FakeInLine("exit", true);

                var c = new ConsoleApplication(mocks.In, mocks.Out);
                c.Run();
                Assert.AreEqual("> TODO: implement this!", mocks.FakeOutLine(true));
                Assert.AreEqual("and this", mocks.FakeOutLine());
                Assert.AreEqual("and so on...", mocks.FakeOutLine());
            }
        }
    }
}
