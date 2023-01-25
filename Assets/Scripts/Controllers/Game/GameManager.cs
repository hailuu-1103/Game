namespace Controllers.Game
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Controllers.Framework;
    using Models;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;
    using Utilities;

    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            // register to receive events
            this.RegisterListener(EventID.OnSelectDarkMode, (param) => this.OnDarkMode(new RectTransform()));
            this.RegisterListener(EventID.OnSelectColorBlindMode, (param) => this.OnColorBlindMode(new RectTransform()));
            Utility.GetRandomWordInDictionary(ref this.targetWord, this.wordData);
            Common.Log(this.targetWord);
            this.DisablePopUp();
            this.DisablePanels();
        }

        #region Init, Config

        [Header("UI Elements")] public List<Text> texts = new(); // Text fill in screen

        public GameObject warningPopUp; // Pop up appears when user inputs word not in the list 
        public float      popUpTime = 2.0f; // Pop up time appears
        public GameObject settingPanel; // Panel setting
        public GameObject parentSpawn; // Use to spawn column child
        public GameObject column4CTilesPrefab; // Column to generate tiles 
        public GameObject column5CTilesPrefab; // Column to generate tiles 
        public GameObject column6CTilesPrefab; // Column to generate tiles 
        public GameObject column7CTilesPrefab; // Column to generate tiles 
        public GameObject btn4Letters; // User choose 4 letters per word to play
        public GameObject btn5Letters; // User choose 5 letters per word to play
        public GameObject btn6Letters; // User choose 6 letters per word to play
        public GameObject btn7Letters; // User choose 7 letters per word to play
        public Sprite     btOnSwitch; // On Switch image
        public Sprite     btOffSwitch; // Off Switch image
        public GameObject tutorialImg; // Show when user wants to see tutorial
        public GameObject txtLetters;
        public GameObject txtLanguage;
        public GameObject txtDarkMode;
        public GameObject txtColorBlindMode;
        public GameObject txtTutorial;
        public GameObject txtShop;
        public GameObject txtRestorePurchase;
        public GameObject txtSocial;

        private          GameObject    spawnObj; // Use to swap tile
        private          GameObject    previousTiles; // Previous column object
        private          List<string>  words = new(); // List of words
        private          string        word  = ""; // Each word
        private          WordState[]   states; // State of each character
        private          WordData      wordData        = new(); // Dictionary of word
        private readonly StringBuilder keyboard        = new("ABCDEFGHIJKLMNOPQRSTUVWXYZ"); // All character of keyboard
        private const    string        InitialFilePath = "5lettersToJson.json"; // File path to read initially
        private          int           inputClickCount; // Times of character user input
        private          int           numberOfWords; // Number of words user input
        private          int           targetLetters = 5; // Number of letters in each word user has to guess

        private bool   isEntered; // Is user press on Enter?
        private bool   isInDictionary; // Is that word in Dictionary?
        private bool   isHided; // Is user click to generate new tiles?
        private bool   isDarkMode; // Is user use dark mode?
        private bool   isColorBlindMode; // Is user use color blind mode?
        private string targetWord; // A Word that user has to guess

        private const int NumberOfRows = 6; // Six words to guess

        #endregion

        #region Singleton

        private static          GameManager uniqueInstance;
        private static readonly object      LockObject = new();

        public static GameManager Instance
        {
            get
            {
                // instance not exist, then create new one
                if (uniqueInstance == null)
                    // Avoid multi-threading problem
                    lock (LockObject)
                    {
                        if (uniqueInstance == null)
                        {
                            // create new GameObject, and add GameManager component
                            var singletonObject = new GameObject();
                            uniqueInstance       = singletonObject.AddComponent<GameManager>();
                            singletonObject.name = "Singleton - GameManager";
                        }
                    }

                return uniqueInstance;
            }
        }

        private void Awake()
        {
            this.spawnObj = this.GenerateTiles(5); // Generate 5 letters of each word initially
            this.wordData = Utility.LoadWord(InitialFilePath);
            // Check if there's another instance already exist in scene

            if (uniqueInstance != null && uniqueInstance.GetInstanceID() != this.GetInstanceID())
                Destroy(this.gameObject);
            else
                // Set instance
                uniqueInstance = this;
        }

        #endregion

        #region Event Callback

        #region Game Play

        /* Events in Game Play */

        /// <summary>
        ///     User input character
        /// </summary>
        /// <param name="t">Transform of button</param>
        public void OnInputCharacterButton(Transform t)
        {
            SoundManager.Instance.PlaySfxSound("button_click");
            if (this.inputClickCount != 0 && this.inputClickCount % this.targetLetters == 0 && !this.isEntered) return;
            this.texts[this.inputClickCount].text =  t.name; // Fill Text
            this.inputClickCount                  += 1;
            this.word                            += t.name;
            if (this.word.Length == this.targetLetters) // If word receive enough characters
            {
                if (Utility.IsWordInDictionary(this.word, this.wordData))
                {
                    this.isInDictionary = true;
                    var tempWord = this.word;
                    this.words.Add(tempWord);
                    this.numberOfWords += 1;
                    this.word          =  ""; // re-init word
                }
                else
                {
                    this.isInDictionary = false;
                }
            }
        }

        public void OnEnterButton()
        {
            if (this.isInDictionary)
            {
                this.isEntered = !this.isEntered;
                this.FillStateToWord();
                this.FillColorToWord();
                this.FillColorToKeyboard();
                if (this.CheckWinGame())
                {
                    SoundManager.Instance.PlaySfxSound("correct_word");
                }

                this.states = new WordState[this.targetLetters]; // re-init States
            }
            else
            {
                // The input word not in dictionary
                this.EnablePopup();
                this.StartCoroutine(this.WaitThenRestoreTime());
            }
        }

        public void OnBackspaceButton()
        {
            if (this.isEntered) return; // If user don't submit the word yet
            this.word                            =  this.word.Length > 0 ? this.word.Remove(this.word.Length - 1) : null;
            this.inputClickCount                  -= 1;
            this.texts[this.inputClickCount].text =  ""; // Set blank text
        }

        public void OnSettingButton()
        {
            this.FillColorToSelectedLetterButton();
            this.EnableSettingPanel();
        }

        #endregion

        #region Settings

        /* Events in Settings */

        public void OnExitSettingButton() { this.DisablePanels(); }

        public void OnExitTutorialButton() { this.DisableTutorial(); }

        /// <summary>
        ///     User select letters to play
        ///     Exp: Select 4 -> generate 4 letters game play
        /// <param name="t">Transform of button</param>
        /// </summary>
        public void OnSelectLetterButton(Transform t)
        {
            this.ClearAllKeyboardColor();
            this.numberOfWords = 0;
            this.texts         = new List<Text>(); // Re-init text to fill
            this.isHided       = !this.isHided;
            if (this.isHided)
            {
                this.Init(t);
                Common.Log(this.targetWord);
            }
            else
            {
                this.Init(t);
                Common.Log(this.targetWord);
            }
        }

        public void OnDarkMode(Transform t)
        {
            this.isDarkMode = !this.isDarkMode;
            this.ChangeTextInButtonDarkMode(this.btn4Letters);
            this.ChangeTextInButtonDarkMode(this.btn5Letters);
            this.ChangeTextInButtonDarkMode(this.btn6Letters);
            this.ChangeTextInButtonDarkMode(this.btn7Letters);
            this.ChangeTextDarkMode(this.txtLetters);
            this.ChangeTextDarkMode(this.txtLanguage);
            this.ChangeTextDarkMode(this.txtDarkMode);
            this.ChangeTextDarkMode(this.txtColorBlindMode);
            this.ChangeTextDarkMode(this.txtTutorial);
            this.ChangeTextDarkMode(this.txtShop);
            this.ChangeTextDarkMode(this.txtRestorePurchase);
            this.ChangeTextDarkMode(this.txtSocial);
            this.ChangeSwitchSprite(t, this.isDarkMode);
        }

        public void OnColorBlindMode(Transform t)
        {
            this.isColorBlindMode = !this.isColorBlindMode;
            this.ChangeSwitchSprite(t, this.isColorBlindMode);
        }

        public void OnTutorialClick() { this.EnableTutorial(); }

        #endregion

        #endregion

        #region GERERATE TILE

        /// <summary>
        /// Generate tiles base on number of letters of each word
        /// </summary>
        /// <param name="numberOfLetters">number of letters of each word</param>
        /// <returns>return Column tile object, return null if no tile spawn</returns>
        private GameObject GenerateTiles(int numberOfLetters)
        {
            if (numberOfLetters == 4)
            {
                return this.SpawnColumn(numberOfLetters, this.column4CTilesPrefab);
            }

            if (numberOfLetters == 5)
            {
                return this.SpawnColumn(numberOfLetters, this.column5CTilesPrefab);
            }

            if (numberOfLetters == 6)
            {
                return this.SpawnColumn(numberOfLetters, this.column6CTilesPrefab);
            }

            if (numberOfLetters == 7)
            {
                return this.SpawnColumn(numberOfLetters, this.column7CTilesPrefab);
            }

            return null;
        }

        /// <summary>
        /// Spawn 6 rows of column and control its Text 
        /// </summary>
        /// <param name="numberOfLetters">Number of letters in each word</param>
        /// <param name="prefab">Prefab to spawn</param>
        /// <returns>Column object</returns>
        private GameObject SpawnColumn(int numberOfLetters, GameObject prefab)
        {
            this.spawnObj      = Instantiate(prefab, this.parentSpawn.transform, false);
            this.spawnObj.name = "Column";
            for (int i = 1; i <= NumberOfRows; i++)
            {
                for (int j = 1; j <= numberOfLetters; j++)
                {
                    var obj = GameObject.Find("Text" + i + j);
                    this.texts.Add(obj.GetComponent<Text>());
                }
            }

            return this.spawnObj;
        }

        #endregion

        #region UI

        private static readonly Color OnDarkModeText  = new(215, 215, 217, 255);
        private static readonly Color OffDarkModeText = Color.black;
        private static readonly Color GreenColor      = Color.green; // set to correct position found character
        private static readonly Color YellowColor     = Color.yellow; // set to incorrect position found character
        private static readonly Color GrayColor       = Color.gray; // set to not found character
        private static readonly Color WhiteColor      = Color.white; // set to unused character

        /// <summary>
        /// Change text corresponding to dark mode selected
        /// </summary>
        /// <param name="obj">Game object to change text color</param>
        private void ChangeTextInButtonDarkMode(GameObject obj)
        {
            var textToChange = obj.GetComponentInChildren<Text>();

            if (this.isDarkMode)
            {
                textToChange.color = OnDarkModeText;
            }
            else
            {
                textToChange.color = OffDarkModeText;
            }
        }

        private void ChangeTextDarkMode(GameObject obj)
        {
            var textToChange = obj.GetComponent<Text>();
            if (this.isDarkMode)
            {
                textToChange.color = OnDarkModeText;
            }
            else
            {
                textToChange.color = OffDarkModeText;
            }
        }

        /// <summary>
        ///     Fill state to input word
        /// </summary>
        private void FillStateToWord()
        {
            var inputWord = this.words[^1];
            this.states = Utility.AddStateToWord(inputWord, this.targetWord, this.targetLetters);
        }

        /// <summary>
        ///     Fill color to input word
        /// </summary>
        private void FillColorToWord()
        {
            var length = this.states.Length;
            for (var i = 0; i < length; i++)
            {
                var middleObj    = GameObject.Find("Text" + this.numberOfWords + (i + 1)); // Text Fill in the middle screen
                var btnMiddleObj = middleObj.transform.parent.gameObject; // Button Object
                this.FillColorByState(i, btnMiddleObj);
            }
        }

        /// <summary>
        ///     Fill color to each character
        ///     If character is in Correct Position, fill its color green
        ///     If character is in Incorrect Position, fill its color yellow
        ///     If character is in Not found, fill its color grey
        /// </summary>
        /// <param name="i">Index in iteration</param>
        /// <param name="btnMiddleObj">Button in the "Mid" Game Object</param>
        private void FillColorByState(int i, GameObject btnMiddleObj)
        {
            var state = this.states[i];
            if (state == WordState.CorrectPositionFound)
                btnMiddleObj.GetComponent<Image>().color = GreenColor;
            else if (state == WordState.IncorrectPositionFound)
                btnMiddleObj.GetComponent<Image>().color = YellowColor;
            else
                btnMiddleObj.GetComponent<Image>().color = GrayColor;
        }

        /// <summary>
        /// Fill green color to selected letter button
        /// Fill white color to unselected letter button
        /// </summary>
        private void FillColorToSelectedLetterButton()
        {
            if (this.targetLetters == 4)
            {
                this.btn4Letters.GetComponent<Image>().color = GreenColor;
                this.btn5Letters.GetComponent<Image>().color = WhiteColor;
                this.btn6Letters.GetComponent<Image>().color = WhiteColor;
                this.btn7Letters.GetComponent<Image>().color = WhiteColor;
            }

            if (this.targetLetters == 5)
            {
                this.btn4Letters.GetComponent<Image>().color = WhiteColor;
                this.btn5Letters.GetComponent<Image>().color = GreenColor;
                this.btn6Letters.GetComponent<Image>().color = WhiteColor;
                this.btn7Letters.GetComponent<Image>().color = WhiteColor;
            }

            if (this.targetLetters == 6)
            {
                this.btn4Letters.GetComponent<Image>().color = WhiteColor;
                this.btn5Letters.GetComponent<Image>().color = WhiteColor;
                this.btn6Letters.GetComponent<Image>().color = GreenColor;
                this.btn7Letters.GetComponent<Image>().color = WhiteColor;
            }

            if (this.targetLetters == 7)
            {
                this.btn4Letters.GetComponent<Image>().color = WhiteColor;
                this.btn5Letters.GetComponent<Image>().color = WhiteColor;
                this.btn6Letters.GetComponent<Image>().color = WhiteColor;
                this.btn7Letters.GetComponent<Image>().color = GreenColor;
            }
        }

        /// <summary>
        ///     Fill color responsive in keyboard
        ///     If keyboard character is in Correct Position, fill its color green
        ///     If keyboard character is in Incorrect Position, fill its color yellow
        ///     If keyboard character is in Not found, fill its color grey
        /// </summary>
        private void FillColorToKeyboard()
        {
            var listOfInputCharacter = this.GetInputCharacter();
            for (var i = 0; i < listOfInputCharacter.Count; i++)
            {
                var bottomObj    = GameObject.Find(listOfInputCharacter[i] + "txt");
                var btnBottomObj = bottomObj.transform.parent.gameObject;
                this.FillColorByState(i, btnBottomObj);
            }
        }

        /// <summary>
        /// Change sprite of toggle
        /// </summary>
        /// <param name="t">Transform of toggle</param>
        /// <param name="mode">Mode to change</param>
        private void ChangeSwitchSprite(Transform t, bool mode)
        {
            if (mode)
            {
                t.transform.gameObject.GetComponent<Image>().sprite = this.btOnSwitch;
            }
            else
            {
                t.transform.gameObject.GetComponent<Image>().sprite = this.btOffSwitch;
            }
        }

        /// <summary>
        ///     Get word base on input characters
        /// </summary>
        /// <returns>Word base on input characters</returns>
        private List<string> GetInputCharacter()
        {
            var list = new List<string>();
            for (var i = 0; i < this.targetLetters; i++)
                list.Add(this.words[^1][i].ToString());
            return list;
        }

        private void DisablePanels()
        {
            this.settingPanel.SetActive(false);
            this.tutorialImg.SetActive(false);
        }

        private void EnableSettingPanel() { this.settingPanel.SetActive(true); }

        private void DisablePopUp() { this.warningPopUp.SetActive(false); }

        private void EnablePopup() { this.warningPopUp.SetActive(true); }

        private void DisableTutorial() { this.tutorialImg.SetActive(false); }
        private void EnableTutorial()  { this.tutorialImg.SetActive(true); }

        private IEnumerator WaitThenRestoreTime()
        {
            yield return new WaitForSecondsRealtime(this.popUpTime);
            this.DisablePopUp();
        }

        #endregion

        #region RE-INIT

        private const string FilePostfix = "lettersToJson.json";

        /// <summary>
        /// Clear all keyboard color
        /// </summary>
        private void ClearAllKeyboardColor()
        {
            for (int i = 0; i < this.keyboard.Length; i++)
            {
                var bottomObj = GameObject.Find(this.keyboard[i].ToString());
                bottomObj.GetComponent<Image>().color = Color.white;
            }
        }

        /// <summary>
        /// Initialize corresponding dictionary
        /// Hide previous tiles
        /// Create new random word
        /// </summary>
        /// <param name="t">Transform of button</param>
        private void Init(Transform t)
        {
            this.spawnObj.SetActive(false); // Hide previous tiles
            var filePrefix = t.GetChild(0).gameObject.GetComponent<Text>().text; // Get number of letters
            this.targetLetters  = int.Parse(filePrefix); // Reset target letters of each word
            this.wordData       = Utility.LoadWord(filePrefix + FilePostfix); // Initialize corresponding dictionary 
            this.words          = new(); // Re-init list
            this.inputClickCount = 0; // Re-init input click
            Utility.GetRandomWordInDictionary(ref this.targetWord, this.wordData);
            this.previousTiles = this.GenerateTiles(this.targetLetters);
            this.DisablePanels();
        }

        #endregion

        #region WIN GAME & LOSE GAME

        private bool CheckWinGame()
        {
            var result = true;
            foreach (var wordState in this.states)
            {
                if (wordState != WordState.CorrectPositionFound)
                {
                    result = false;
                }
            }

            return result;
        }

        private void LoseGame() { }

        /// <summary>
        ///     Khi user nhấn vào setting, tắt setting sẽ quay lại trạng thái chơi
        ///     Sau khi user mở lại trò chơi, sẽ có UI hiện lên và hỏi user có muốn quay lại scene đang chơi không?
        ///     Nếu có thì sẽ load lại trạng thái cuối cùng, không thì sẽ reset scence và chơi lại
        /// </summary>
        private void OnResumeButton() { }

        /// <summary>
        ///     Khi user nhấn vào button -> hiển thị Confirm UI. Đồng ý thì sẽ load lại scene, không thì tắt UI
        /// </summary>
        private void OnReplayButton() { }

        #endregion
    }
}