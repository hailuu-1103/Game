using System;

[Serializable]
public class CertainWord
{
    public CertainWord()
    {
        Word = "";
    }

    public string Word { get; set; }

    public void AddCharacterToWord(string character)
    {
        Word += character;
    }

    /// <summary>
    /// Remove last character of word
    /// </summary>
    /// <returns>Word after removing</returns>
    public string RemoveCharacterFromWord()
    {
        return Word.Length > 0 ? Word.Remove(Word.Length - 1) : null;
    }
    public void Clear()
    {
        Word = null;
    }

    public override string ToString()
    {
        return Word;
    }
}