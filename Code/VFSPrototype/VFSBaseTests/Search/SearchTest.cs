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
        
        private RootFolder root;
        private Folder topFolderBli;
        private Folder subBliFoo;

        [TestInitialize]
        public void FillServiceIndex()
        {
            root = new RootFolder();

            topFolderBli = new Folder("bli") {Parent = root, BlockNumber = 0};
            var topFolderBla = new Folder ("bla") { Parent = root, BlockNumber = 1};
            var topFolderBlub = new Folder ("blup") { Parent = root, BlockNumber = 2};
            var topFolderBbb = new Folder ("bbb") { Parent = root, BlockNumber = 3};
            _service.AddToIndex(topFolderBli);
            _service.AddToIndex(topFolderBla);
            _service.AddToIndex(topFolderBlub);
            _service.AddToIndex(topFolderBbb);

            subBliFoo = new Folder("foo") { Parent = topFolderBli, BlockNumber = 4};
            var subBliFooCapital = new Folder("FOO") { Parent = topFolderBli, BlockNumber = 5};
            var subBliBar = new Folder("bar") { Parent = topFolderBli, BlockNumber = 6};
            _service.AddToIndex(subBliFoo);
            _service.AddToIndex(subBliFooCapital);
            _service.AddToIndex(subBliBar);

            var subsubBliFooAaa = new Folder("aaa") { Parent = subBliFoo, BlockNumber = 7};
            var subsubBliFooBbb = new Folder("bbb") { Parent = subBliFoo, BlockNumber = 8};
            var subsubBliFooCcc = new Folder("Ccc") { Parent = subBliFoo, BlockNumber = 9};
            _service.AddToIndex(subsubBliFooAaa);
            _service.AddToIndex(subsubBliFooBbb);
            _service.AddToIndex(subsubBliFooCcc);

            var subBliFooFile = new VFSFile("File") { Parent = subBliFoo, BlockNumber = 10};
            var subBliFooFile2 = new VFSFile("File2") { Parent = subBliFoo, BlockNumber = 11};
            var subBliFooFile3 = new VFSFile("File3") { Parent = subBliFoo, BlockNumber = 12};
            var subBliFooBar = new Folder("bar") { Parent = subBliFoo, BlockNumber = 13};
            _service.AddToIndex(subBliFooFile);
            _service.AddToIndex(subBliFooFile2);
            _service.AddToIndex(subBliFooFile3);
            _service.AddToIndex(subBliFooBar);
        }

        [TestMethod]
        public void TestInFolder()
        {
            Assert.AreEqual (0, _service.Search(new SearchOptions { Keyword = "bar", RecursionDistance = 0, RestrictToFolder = root}).Count());
            Assert.AreEqual (1, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 1, RestrictToFolder = root }).Count ());
            Assert.AreEqual (2, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 2, RestrictToFolder = root }).Count ());
            Assert.AreEqual (1, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 0, RestrictToFolder = topFolderBli }).Count ());
            Assert.AreEqual (2, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 1, RestrictToFolder = topFolderBli }).Count ());
            Assert.AreEqual (1, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 1, RestrictToFolder = subBliFoo }).Count ());
        }

        [TestMethod]
        public void TestSimpleSearch()
        {
            Assert.AreEqual(0, _service.Search(new SearchOptions { Keyword = "Test" }).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "bli" }).Count());
            Assert.AreEqual(1, _service.Search(new SearchOptions { Keyword = "aaa" }).Count());
            Assert.AreEqual(2, _service.Search(new SearchOptions { Keyword = "bbb" }).Count());
            Assert.AreEqual(3, _service.Search(new SearchOptions { Keyword = "bl" }).Count());
            Assert.AreEqual(7, _service.Search(new SearchOptions { Keyword = "b" }).Count());
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
