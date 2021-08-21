using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Validator.Controllers
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
            for (int i = 1; i < ruleDt.Rows.Count; i++)
            {
                for (int j = 0; j < ruleDt.Columns.Count; j++)
                {
                    if (ruleDt.Rows[i][j].ToString() != "*")
                        insertSubstringStartingAt( j, ruleDt);
                }
            }
        }
        public void insertSubstringStartingAt(int j,  DataTable ruleDt)
        {
            //ruleDt.Rows[i][j].ToString()

            TrieNode node = root;
            for (int idx = j; idx < ruleDt.Columns.Count; idx++)
            {
                string cell = ruleDt.Rows[idx][idx].ToString();
                if (!node.Children.ContainsKey(cell))
                {
                    node.Children.Add(cell, new TrieNode());
                }
                node = node.Children[cell];
            }

            node.Children[endSymbol] = null;
        }


        public bool Contains(DataTable lineitemsDt,int i)
        {
            TrieNode node = root;
            for (int j = 1; j < lineitemsDt.Columns.Count; j++)
            {
                string cell = lineitemsDt.Rows[i][j].ToString();
                if (!node.Children.ContainsKey(cell))
                {
                    return false;
                }
                node = node.Children[cell];
            }
            return node.Children.ContainsKey(endSymbol);
        }
    }
}
