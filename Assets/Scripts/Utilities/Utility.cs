using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers.Framework;
using DefaultNamespace;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

namespace Utilities
{
    [Serializable]
    public class Utility
    {
        #region STRING HANDLING

        
        /// <summary>
        ///     Add state to input word
        /// </summary>
        /// <param name="inputWord">Word that user input</param>
        /// <param name="targetWord">Word in Dictionary</param>
        /// <param name="targetLetters">Number of letters of each word</param>
        public WordState[] AddStateToWord(string inputWord, string targetWord, int targetLetters)
        {
            var states = new WordState[targetLetters];
            inputWord = inputWord.ToLower();
            targetWord = targetWord.ToLower();
            for (var i = 0; i < inputWord.Length; i++)
                if (targetWord.Contains(inputWord[i]))
                {
                    if (inputWord[i] == targetWord[i])
                        states[i] = WordState.CORRECTPOSITIONFOUND;
                    else
                        states[i] = WordState.INCORRECTPOSITIONFOUND;
                }

            return states;
        }

        #endregion

        #region RESOURCE HANDLING

        /// <summary>
        ///     Get random word in dictionary
        /// </summary>
        /// <returns>Word in dictionary</returns>
        public void GetRandomWordInDictionary(ref string word, WordData wordData)
        {
            wordData.WordList = wordData.WordList.ConvertAll(d => d.ToLower()); // Convert all word to lowercase
            var length = wordData.WordList.Count;
            if (length > 0)
            {
                var random = new Random();
                word = wordData.WordList[random.Next(length)];
            }
        }

        /// <summary>
        ///     Check input word in dictionary
        /// </summary>
        /// <param name="inputWord">Word that user input</param>
        /// <param name="data">List of word</param>
        /// <returns>true if word exist in dictionary, false otherwise</returns>
        public bool IsWordInDictionary(string inputWord, WordData data)
        {
            var list = data.WordList;
            list = list.ConvertAll(d => d.ToLower()); // Convert all word to lowercase
            if (list.Contains(inputWord.ToLower())) return true;
            return false;
        }

        #endregion

        #region FILE HANDLING

        /// <summary>
        ///     Read file and parse to object
        /// </summary>
        /// <param name="filePath">File path by string</param>
        public WordData ReadWord(string filePath)
        {
            var path = Application.dataPath + "/Resources/" + filePath;
            if (File.Exists(path))
            {
                var reader = new StreamReader(path);
                var data = reader.ReadToEnd().ToLower();
                return JsonConvert.DeserializeObject<WordData>(data);
            }

            Debug.LogError("Save file not found in " + path);
            return null;
        }

        /// <summary>
        ///     Load file and parse to object
        /// </summary>
        /// <param name="filePath">File path by string</param>
        /// <returns>Object store data</returns>
        public WordData LoadWord(string filePath, int type)
        {
            var wordData = new WordData();
            var path = Application.dataPath + "/Resources/" + filePath;
            if (File.Exists(path))
            {
                var reader = new StreamReader(path);
                var data = type == 1 ? reader.ReadToEnd() : reader.ReadToEnd().Replace("\r", "").Replace("\n", " ");
                var words = data.Split(' ').ToList();
                foreach (var word in words) wordData.AddToList(word);
                return wordData;
            }

            Debug.LogError("Save file not found in " + path);
            return null;
        }

        /// <summary>
        ///     Convert a object to JSON file
        /// </summary>
        /// <param name="wordData">Object store data</param>
        /// <param name="filePath">Name of the JSON file</param>
        public void ConvertToJson(WordData wordData, string filePath)
        {
            var setting = new JsonSerializerSettings();
            setting.Formatting = Formatting.Indented;
            setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            var json = JsonConvert.SerializeObject(wordData, setting);
            var path = Path.Combine(Application.dataPath + "/Resources/", filePath);
            File.WriteAllText(path, json);
        }

        #endregion
    }
}