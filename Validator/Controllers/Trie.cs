using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Trie
{
    public class TrieNode
    {
        public Dictionary<string, TrieNode> Children = new Dictionary<string, TrieNode>();
    }

    public class SuffixTrie
    {
        public TrieNode root = new TrieNode();
        public string endSymbol = "#";

        public SuffixTrie(DataTable ruleDt)
        {
            PopulateSuffixTrieFrom(ruleDt);
        }

        public void PopulateSuffixTrieFrom(DataTable ruleDt)
        {
            for (int i = 0; i < ruleDt.Rows.Count; i++)
            {

                Console.WriteLine(i);
                insertSubstringStartingAt(0, i, ruleDt);
                 
            }
        }
        public void insertSubstringStartingAt(int j, int i, DataTable ruleDt)
        {
            //ruleDt.Rows[i][j].ToString()

            TrieNode node = root;
            for (int idx = j; idx < ruleDt.Columns.Count; idx++)
            {
                // if (ruleDt.Rows[i][j].ToString() == "*") continue;

                string cell = ruleDt.Rows[i][idx].ToString();

                if (!node.Children.ContainsKey(cell))
                {
                    node.Children.Add(cell, new TrieNode());
                }
                node = node.Children[cell];
            }

            node.Children[endSymbol] = null;
        }


        public bool Contains(DataTable lineitemsDt, int i)
        {
            TrieNode node = root;
            for (int j = 0; j < lineitemsDt.Columns.Count; j++)
            {

                string cell = lineitemsDt.Rows[i][j].ToString();
                if ((!node.Children.ContainsKey(cell)) && !node.Children.ContainsKey("*"))
                {
                    return false;
                }
                if (node.Children.ContainsKey(cell))
                    node = node.Children[cell];
                else
                    node = node.Children["*"];
            }
            return node.Children.ContainsKey(endSymbol);
        }
    }
}
