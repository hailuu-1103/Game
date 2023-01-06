using System;
using System.Collections.Generic;

[Serializable]
public class Words : CertainWord
{
    public Words()
    {
        WordList = new List<CertainWord>();
    }

    public List<CertainWord> WordList { get; set; }

    public void AddWordToList(CertainWord word)
    {
        WordList.Add(word);
    }

    public void SetAllWordsToLower()
    {
        foreach (var word in WordList)
        {
            word.Word.ToLower();
        }
    }
}