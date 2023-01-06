using System;
using System.Collections.Generic;

namespace DefaultNamespace
{
    [Serializable]
    public class WordData
    {
        public WordData()
        {
            WordList = new List<string>();
        }

        public List<string> WordList { get; set; }

        public void AddToList(string word)
        {
            WordList.Add(word);
        }
    }
}