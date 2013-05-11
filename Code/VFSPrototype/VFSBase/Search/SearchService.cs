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
            Index("/"); // See comment below
            // Does not work (yet)... Low prio...? 
            //Index("/");
            //Task.Run(() => Index("/"));
            //new Thread(ThreadDelegate).Start("/");
        }

        private void ThreadDelegate(object o)
        {
            Index(o.ToString());
        }

        private void Index(string path)
        {
            path = path.TrimEnd('/');
            lock (_lock)
            {
                foreach (var file in _manipulator.Files(path))
                {
                    _indexService.AddToIndex(path + "/" + file);
                }
                foreach (var folder in _manipulator.Folders(path.TrimEnd('/')))
                {
                    _indexService.AddToIndex(path + "/" + folder + "/");
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
            // Tests didn't run... TODO: fix this (and run tests after fix)
            //
            // About concurrency: this cannot work concurrently!
            // Scenario:
            // T1: Create folder "a"
            // T1: Create folder "b"
            // T2: Start indexing "a"
            // T1: Delete folder "a"
            // T2: Start indexing contents of "a" => "a" deleted => no contents => exception!
            //
            // Possible solutions:
            //  1) Do it synchonously
            //  2) Do it only when searching
            //  3) Use locking in the fileSystemTextManipulator

            if (!_manipulator.Exists(path)) return;

            AddToIndex(path);

            // Does not work (yet)... Low prio...? 
            // if (_manipulator.IsDirectory(path.TrimEnd('/'))) Index(path.TrimEnd('/'));
            // if (_manipulator.IsDirectory(path)) Task.Run(() => Index(path));
            if (_manipulator.IsDirectory(path.TrimEnd('/'))) Index(path);
            //new Thread(ThreadDelegate).Start(path);
        }
    }
}
