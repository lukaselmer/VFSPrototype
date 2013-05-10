using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    class SearchService : ISearchService
    {
        private readonly IFileSystemTextManipulator _manipulator;
        private readonly IndexService _indexService;
        private readonly object _lock = new object();


        public SearchService (IFileSystemTextManipulator manipulator)
        {
            _manipulator = manipulator;
            _indexService = new IndexService();
        }

        public void StartIndexing() {
            Task.Run(() => Index("/"));
        }

        private void Index(string path)
        {
            lock (_lock)
            {
                foreach (var file in _manipulator.Files(path))
                {
                    _indexService.AddToIndex(path + "/" + file);
                }
                foreach (var folder in _manipulator.Folders(path))
                {
                    _indexService.AddToIndex(path + "/" + folder);
                    Index(path + "/" + folder);
                }
            }
        }

        public IEnumerable<string> Search(SearchOptions searchOptions)
        {
            lock (_lock)
            {
                return _indexService.Search(searchOptions).Where(_manipulator.Exists);
            }
        }

        public void AddToIndex(string path)
        {
            lock (_lock)
            {
                _indexService.AddToIndex(path);
            }
        }

        public void AddToIndexRecursive(string path)
        {
            AddToIndex(path);
            Task.Run(() => Index(path));
        }
    }
}
