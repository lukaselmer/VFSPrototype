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
    public class IndexService
    {
        private readonly SuffixTree _flatNames = new SuffixTree();

        internal IEnumerable<IIndexNode> Search(SearchOptions searchOptions)
        {
            var result = _flatNames.Search(searchOptions);

            var restrictFolderPath = (searchOptions.RestrictToFolder == null) 
                                     ? "" 
                                     : GetPath(searchOptions.RestrictToFolder);
                
            return result.Where(indexNode => IsInFolder(restrictFolderPath, GetPath(indexNode), searchOptions.RecursionDistance));
        }

        private static string GetPath(IIndexNode node)
        {
            var sb = new StringBuilder();
            
            while (node != null)
            {
                sb.Insert(0, node.Name + "/");
                node = node.Parent;
            }
            return sb.ToString().TrimEnd('/');
        }

        private static bool IsInFolder (string restrictFolderPath, string nodePath, int recursionDistance)
        {
            var recursionDepth = 0;
            while ((recursionDistance == -1 || recursionDepth <= recursionDistance) && nodePath.Length > 0)
            {
                nodePath = nodePath.Substring (0, nodePath.LastIndexOf ("/", StringComparison.CurrentCulture));

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

        internal void AddToIndex(IIndexNode node)
        {
            _flatNames.Insert(node.Name, node);
        }
        
        internal void RemoveFromIndex(IIndexNode node)
        {
            _flatNames.Remove(node.Name, node);
        }
    }
}
