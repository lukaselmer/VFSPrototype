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

            return result.Where(path => IsInFolder(searchOptions.RestrictToFolder ?? "", path, searchOptions.RecursionDistance));
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

        //private static bool IsInFolder(Folder folder, IIndexNode node, int recursionDistance)
        //{
        //    var parent = node.Parent;

        //    var recursionDepth = 0;
        //    while (parent != null && (recursionDistance == -1 || recursionDepth <= recursionDistance))
        //    {
        //        if (parent.Equals(folder))
        //            return true;

        //        recursionDepth++;
        //        parent = parent.Parent;
        //    }
        //    return false;
        //}

        private static string GetName(string path)
        {
            return path.Substring(path.TrimEnd('/').LastIndexOf("/", StringComparison.Ordinal) + 1);
        }

        public void AddToIndex(string path)
        {
            _flatNames.Insert(GetName(path), path);
        }
    }
}
