using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    class SearchService : ISearchService
    {
        private readonly IFileSystemTextManipulator _manipulator;
        private readonly IndexService _indexService;
        private readonly object _lock = new object();


        public SearchService(IFileSystemTextManipulator manipulator)
        {
            _manipulator = manipulator;
            _indexService = new IndexService();
        }

        public void StartIndexing()
        {
            // Does not work (yet)... Low prio...? 
            //Index("/");
            //Task.Run(() => Index("/"));
            new Thread (ThreadDelegate).Start ("/");
        }

        private void ThreadDelegate (object o)
        {
            Index(o.ToString());
        }

        private void Index(string path)
        {
            lock (_lock)
            {
                foreach (var file in _manipulator.Files(path))
                {
                    _indexService.AddToIndex(path + file);
                }
                foreach (var folder in _manipulator.Folders(path.TrimEnd('/')))
                {
                    _indexService.AddToIndex (path + folder + "/");
                    Index (path + folder + "/");
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
            if (!_manipulator.Exists(path)) return;

            AddToIndex(path);

            // Does not work (yet)... Low prio...? 
            // if (_manipulator.IsDirectory(path.TrimEnd('/'))) Index(path.TrimEnd('/'));
            // if (_manipulator.IsDirectory(path)) Task.Run(() => Index(path));
            if (_manipulator.IsDirectory (path.TrimEnd ('/')))
                new Thread (ThreadDelegate).Start (path);
        }
    }
}
