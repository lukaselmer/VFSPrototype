using System.Collections.Generic;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    internal class SuffixTreeNode
    {
        public SuffixTreeNode()
        {
            Nodes = new SortedDictionary<char, SuffixTreeNode>();
            Values = new HashSet<string>();
        }

        public IDictionary<char, SuffixTreeNode> Nodes { get; set; }
        public HashSet<string> Values { get; set; }
    }
}