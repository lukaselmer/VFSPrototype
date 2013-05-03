using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    class IndexingService
    {
        private static IFileSystem _fileSystem;
        private static IndexService _indexService;


        public static void StartIndexing(IndexService indexService, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _indexService = indexService;

            // This does not work properly, we would need to use synchronization
            // Task.Run(() => Index(_fileSystem.Root));
            Index(_fileSystem.Root);
        }

        private static void Index(Folder folder)
        {
            foreach (var item in _fileSystem.Files(folder))
            {
                _indexService.AddToIndex(item);
            }
            foreach (var item in _fileSystem.Folders(folder))
            {
                _indexService.AddToIndex(item);
                Index(item);
            }
        }
    }
}
