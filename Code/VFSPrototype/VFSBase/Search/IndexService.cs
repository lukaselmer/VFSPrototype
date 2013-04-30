using System.Collections.Generic;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    /// <summary>
    /// Scenarios:
    /// All: File name Search
    /// 
    /// Settings
    /// * Case sensivity / insensitivity
    /// * Regex / Wildcards
    /// * "Edit distance"
    /// 
    /// Modes
    /// * Restrict search to folder
    /// * Restrict search to folder and subfolders
    /// </summary>
    class IndexService
    {
        Trie flatNames = new Trie();
        Trie pathNames = new Trie();

        internal void AddToIndex(Folder folder)
        {

        }

        internal void AddToIndex(VFSFile file)
        {

        }

        internal void RemoveFromIndex(Folder folder)
        {

        }

        internal void RemoveFromIndex(VFSFile file)
        {

        }

        internal IEnumerable<IIndexNode> Search(SearchOptions searchOptions)
        {
            yield return null;
        }
    }
}
