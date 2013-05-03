using System.Collections.Generic;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    internal class TrieNode
    {
        public TrieNode()
        {
            Nodes = new SortedDictionary<char, TrieNode>();
            Values = new HashSet<IIndexNode>();
        }

        public IDictionary<char, TrieNode> Nodes { get; set; }
        public HashSet<IIndexNode> Values { get; set; }
    }
}