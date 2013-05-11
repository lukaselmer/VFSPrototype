using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var recursionDepth = 0;
            while ((recursionDistance == -1 || recursionDepth <= recursionDistance) && nodePath.Length > 0)
            {
                nodePath = nodePath.Substring(0, nodePath.LastIndexOf("/", StringComparison.CurrentCulture));

                if (nodePath == restrictFolderPath)
                    return true;

                recursionDepth++;
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
