using System.Collections.Generic;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    internal class Trie
    {
        private TrieNode RootNode { get; set; }

        public Trie()
        {
            RootNode = new TrieNode();
        }

        public IEnumerable<IIndexNode> Search(string path, SearchOptions options)
        {
            var currentNode = RootNode;
            foreach (var c in path)
            {
                if(options.SearchInCurrentFolder)
                    currentNode.Nodes.TryGetValue(c, out currentNode);
                if (currentNode == null) return null;
            }

            return currentNode.Values;
        }

        public void Insert(string path, IIndexNode node)
        {
            var currentNode = RootNode;
            foreach (var c in path)
            {
                TrieNode nextNode;
                currentNode.Nodes.TryGetValue(c, out nextNode);
                if (nextNode == null)
                {
                    nextNode = new TrieNode();
                    currentNode.Nodes.Add(c, nextNode);
                }
                currentNode = nextNode;
            }
            currentNode.Values.AddFirst(node);
        }
    }
}