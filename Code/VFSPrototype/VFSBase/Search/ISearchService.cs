using System.Collections.Generic;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    internal interface ISearchService
    {
        IEnumerable<string> Search(SearchOptions searchOptions);
        void AddToIndex(string path);
        void AddToIndexRecursive(string path);
        void StartIndexing();
    }
}