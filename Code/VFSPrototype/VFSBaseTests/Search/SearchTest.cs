using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VFSBase.Implementation;
using VFSBase.Search;

namespace VFSBaseTests.Search
{
    [TestClass]
    public class SearchTest
    {
        private readonly IndexService _service = new IndexService();

        [TestInitialize]
        public void FillServiceIndex()
        {
            var topFolderBli = new Folder("bli");
            var topFolderBla = new Folder("bla");
            var topFolderBlub = new Folder("blub");
            var topFolderBbb = new Folder("bbb");
            _service.AddToIndex(topFolderBli);
            _service.AddToIndex(topFolderBla);
            _service.AddToIndex(topFolderBlub);
            _service.AddToIndex(topFolderBbb);

            var subBliFoo = new Folder("foo") { Parent = topFolderBli };
            var subBliFooCapital = new Folder("FOO") { Parent = topFolderBli };
            var subBliBar = new Folder("bar") { Parent = topFolderBli };
            _service.AddToIndex(subBliFoo);
            _service.AddToIndex(subBliFooCapital);
            _service.AddToIndex(subBliBar);

            var subsubBliFooAaa = new Folder("aaa") { Parent = subBliFoo };
            var subsubBliFooBbb = new Folder("bbb") { Parent = subBliFoo };
            var subsubBliFooCcc = new Folder("Ccc") { Parent = subBliFoo };
            _service.AddToIndex(subsubBliFooAaa);
            _service.AddToIndex(subsubBliFooBbb);
            _service.AddToIndex(subsubBliFooCcc);

            var subBliFooFile = new VFSFile("File") { Parent = subBliFoo };
            var subBliFooFile2 = new VFSFile("File2") { Parent = subBliFoo };
            var subBliFooFile3 = new VFSFile("File3") { Parent = subBliFoo };
            var subBliFooBar = new Folder("bar") { Parent = subBliFoo };
            _service.AddToIndex(subBliFooFile);
            _service.AddToIndex(subBliFooFile2);
            _service.AddToIndex(subBliFooFile3);
            _service.AddToIndex(subBliFooBar);
        }

        [TestMethod]
        public void TestInFolder()
        {
            Assert.AreEqual(0, _service.Search(new SearchOptions { Keyword = "bar", RecursionDistance = 0, RestrictToFolderPath = "/"}).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "bar", RecursionDistance = 1, RestrictToFolderPath = "/"}).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "bar", RecursionDistance = 2, RestrictToFolderPath = "/"}).Count());
            Assert.AreEqual(2, _service.Search(new SearchOptions { Keyword = "bar", RecursionDistance = 3, RestrictToFolderPath = "/"}).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "bar", RecursionDistance = 0, RestrictToFolderPath = "/bli"}).Count());

            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "foo", CaseSensitive = true }).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "FOO", CaseSensitive = true }).Count());
        }

        [TestMethod]
        public void TestSimpleSearch()
        {
            Assert.AreEqual(0, _service.Search(new SearchOptions { Keyword = "Test" }).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "bli" }).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "aaa" }).Count());
            Assert.AreEqual(2, _service.Search(new SearchOptions { Keyword = "bbb" }).Count());
        }

        [TestMethod]
        public void TestCaseSensitivity()
        {
            Assert.AreEqual(2, _service.Search(new SearchOptions { Keyword = "bar" }).Count());
            Assert.AreEqual(2, _service.Search(new SearchOptions { Keyword = "foo" }).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "foo", CaseSensitive = true}).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "FOO", CaseSensitive = true}).Count());
        }

    }
}
