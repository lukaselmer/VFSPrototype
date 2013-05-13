using System;
using System.Collections.Generic;
using System.Linq;

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
    internal class IndexService
    {
        private readonly SuffixTree _flatNames = new SuffixTree();

        public IEnumerable<string> Search(SearchOptions searchOptions)
        {
            var result = _flatNames.Search(searchOptions);

            return result.Where(path => IsInFolder(searchOptions.RestrictToFolder ?? "", path.TrimEnd('/'), searchOptions.RecursionDistance));
        }

        private static bool IsInFolder(string restrictFolderPath, string nodePath, int recursionDistance)
        {
            nodePath = nodePath.Substring (0, nodePath.LastIndexOf ("/", StringComparison.CurrentCulture));
            if (nodePath.StartsWith (restrictFolderPath)) {
                if (recursionDistance == -1)
                    return true;
                nodePath = nodePath.Remove (0, restrictFolderPath.Length);
                var count = nodePath.Count (c => c == '/');
                return count <= recursionDistance;
            }
            return false;
        }

        private static string GetName(string path)
        {
            path = path.TrimEnd('/');
            return path.Substring(path.LastIndexOf("/", StringComparison.Ordinal) + 1);
        }

        public void AddToIndex(string path)
        {
            _flatNames.Insert(GetName(path), path);
        }
    }
}
