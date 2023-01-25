namespace Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class WordData
    {
        public WordData()
        {
            this.WordList = new List<string>();
        }

        public List<string> WordList { get; set; }

        public void AddToList(string word)
        {
            this.WordList.Add(word);
        }
    }
}