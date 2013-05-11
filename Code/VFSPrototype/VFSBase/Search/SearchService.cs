using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    class SearchService : ISearchService
    {
        private readonly IFileSystemTextManipulator _manipulator;
        private readonly IndexService _indexService;
        private readonly ReaderWriterLockSlim _lock;


        public SearchService(IFileSystemTextManipulator manipulator, ReaderWriterLockSlim readerWriterLock)
        {
            _lock = readerWriterLock;
            _manipulator = manipulator;

            _indexService = new IndexService();
        }

        public void StartIndexing()
        {
            Task.Run(() => Index("/"));
        }

        private void Index(string path)
        {
            path = path.TrimEnd('/');
            _lock.EnterUpgradeableReadLock();
            try
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
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<string> Search(SearchOptions searchOptions)
        {
            return _indexService.Search(searchOptions).Where(_manipulator.Exists);
        }

        public void AddToIndex(string path)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                _indexService.AddToIndex(path);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void AddToIndexRecursive(string path)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_manipulator.Exists(path)) return;

                AddToIndex(path);
                if (_manipulator.IsDirectory(path)) Task.Run(() => Index(path));
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }
}
