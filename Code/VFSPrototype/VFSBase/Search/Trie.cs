using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<IIndexNode> Search(SearchOptions options)
        {
            return Search(RootNode, options);
        }

        private IEnumerable<IIndexNode> Search(TrieNode node, SearchOptions options)
        {
            if (options.Keyword.Length == 0)
                return node.Values;
            
            var c = options.Keyword.First();
            if (options.CaseSensitive == false) {
                TrieNode nextUpperNode;
                TrieNode nextLowerNode;
                node.Nodes.TryGetValue(char.ToLower(c), out nextUpperNode);
                node.Nodes.TryGetValue(char.ToUpper(c), out nextLowerNode);
                if (nextUpperNode == null && nextLowerNode == null) return Enumerable.Empty<IIndexNode>();

                options.Keyword = options.Keyword.Substring(1);

                if (nextUpperNode != null && nextLowerNode != null)
                    return Search(nextLowerNode, options).Union(Search(nextUpperNode, options));

                return Search(nextLowerNode ?? nextUpperNode, options);

            } else {
                TrieNode nextNode;
                node.Nodes.TryGetValue(c, out nextNode);
                if (nextNode == null) return Enumerable.Empty<IIndexNode>();

                options.Keyword = options.Keyword.Substring(1);
                return Search(nextNode, options);
            }
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
                nextNode.Values.Add(node);
                currentNode = nextNode;
            }

            if (path.Length >= 1)
                Insert(path.Substring(1), node);
        }



        public IIndexNode Remove(string path, IIndexNode node)
        {
            var currentNode = RootNode;
            foreach (var c in path)
            {
                TrieNode nextNode;
                currentNode.Nodes.TryGetValue(c, out nextNode);
                if (nextNode == null) return null; // Not found
                if (node != null) currentNode.Values.Remove(node);
                currentNode = nextNode;
            }

            if (path.Length >= 1)
                Remove(path.Substring(1), node);

            return currentNode.Values.Remove(node) ? node : null;
        }
    }
}