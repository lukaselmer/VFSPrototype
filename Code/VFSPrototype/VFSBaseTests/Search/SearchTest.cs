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
        
        private string _root;
        private string _topFolderBli;
        private string _subBliFoo;

        [TestInitialize]
        public void FillServiceIndex()
        {
            _root = "";

            _topFolderBli = "/bli";
            var topFolderBla = "/bla";
            var topFolderBlub = "/blup";
            var topFolderBbb = "/bbb";
            _service.AddToIndex(_topFolderBli);
            _service.AddToIndex(topFolderBla);
            _service.AddToIndex(topFolderBlub);
            _service.AddToIndex(topFolderBbb);

            _subBliFoo = "/bli/foo";
            var subBliFooCapital = "/bli/FOO";
            var subBliBar = "/bli/bar";
            _service.AddToIndex(_subBliFoo);
            _service.AddToIndex(subBliFooCapital);
            _service.AddToIndex(subBliBar);

            var subsubBliFooAaa = "/bli/foo/aaa";
            var subsubBliFooBbb = "/bli/foo/bbb";
            var subsubBliFooCcc = "/bli/foo/Ccc";
            _service.AddToIndex(subsubBliFooAaa);
            _service.AddToIndex(subsubBliFooBbb);
            _service.AddToIndex(subsubBliFooCcc);

            var subBliFooFile = "/bli/foo/File";
            var subBliFooFile2 = "/bli/foo/File2";
            var subBliFooFile3 = "/bli/foo/File3";
            var subBliFooBar = "/bli/foo/bar";
            _service.AddToIndex(subBliFooFile);
            _service.AddToIndex(subBliFooFile2);
            _service.AddToIndex(subBliFooFile3);
            _service.AddToIndex(subBliFooBar);
        }

        [TestMethod]
        public void TestInFolder()
        {
            Assert.AreEqual (0, _service.Search(new SearchOptions { Keyword = "bar", RecursionDistance = 0, RestrictToFolder = _root}).Count());
            Assert.AreEqual (1, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 1, RestrictToFolder = _root }).Count ());
            Assert.AreEqual (2, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 2, RestrictToFolder = _root }).Count ());
            Assert.AreEqual (1, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 0, RestrictToFolder = _topFolderBli }).Count ());
            Assert.AreEqual (2, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 1, RestrictToFolder = _topFolderBli }).Count ());
            Assert.AreEqual (1, _service.Search (new SearchOptions { Keyword = "bar", RecursionDistance = 1, RestrictToFolder = _subBliFoo }).Count ());
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
