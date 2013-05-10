using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VFSBase.Interfaces;

namespace VFSBase.Search
{
    internal class SuffixTree
    {
        private SuffixTreeNode RootNode { get; set; }

        public SuffixTree()
        {
            RootNode = new SuffixTreeNode();
        }

        public IEnumerable<string> Search(SearchOptions options)
        {
            return Search(RootNode, options);
        }

        private IEnumerable<string> Search(SuffixTreeNode node, SearchOptions options)
        {
            if (options.Keyword.Length == 0)
                return node.Values;
            
            var c = options.Keyword.First();
            if (options.CaseSensitive == false) {
                SuffixTreeNode nextUpperNode;
                SuffixTreeNode nextLowerNode;
                node.Nodes.TryGetValue(char.ToLower(c), out nextLowerNode);
                node.Nodes.TryGetValue (char.ToUpper (c), out nextUpperNode);
                if (nextUpperNode == null && nextLowerNode == null) return Enumerable.Empty<string>();

                options.Keyword = options.Keyword.Substring(1);

                if (nextUpperNode != null && nextLowerNode != null)
                    return Search(nextLowerNode, options.Clone()).Union(Search(nextUpperNode, options.Clone()));

                return Search(nextLowerNode ?? nextUpperNode, options.Clone());

            } else {
                SuffixTreeNode nextNode;
                node.Nodes.TryGetValue(c, out nextNode);
                if (nextNode == null) return Enumerable.Empty<string>();

                options.Keyword = options.Keyword.Substring(1);
                return Search(nextNode, options);
            }
        }


        public void Insert(string name, string path)
        {
            var currentNode = RootNode;
            foreach (var c in name)
            {
                SuffixTreeNode nextNode;
                currentNode.Nodes.TryGetValue(c, out nextNode);
                if (nextNode == null)
                {
                    nextNode = new SuffixTreeNode();
                    currentNode.Nodes.Add(c, nextNode);
                }
                nextNode.Values.Add(path);
                currentNode = nextNode;
            }

            if (name.Length >= 1)
                Insert(name.Substring(1), path);
        }
    }
}