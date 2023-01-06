using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Controllers.Framework;
using DefaultNamespace;
using Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace Controllers.Game
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            // register to receive events
            this.RegisterListener(EventID.OnSelectDarkMode, (param) => OnDarkMode(new RectTransform()));
            this.RegisterListener(EventID.OnSelectColorBlindMode, (param) => OnColorBlindMode(new RectTransform()));
            _utility.GetRandomWordInDictionary(ref _targetWord, _wordData);
            Common.Log(_targetWord);
            DisablePopUp();
            DisablePanels();
        }

        #region Init, Config

        [Header("UI Elements")] 
        public List<Text> Texts = new List<Text>(); // Text fill in screen
        public GameObject warningPopUp; // Pop up appears when user inputs word not in the list 
        public float popUpTime = 2.0f; // Pop up time appears
        public GameObject settingPanel; // Panel setting
        public GameObject parentSpawn; // Use to spawn column child
        public GameObject column4CTilesPrefab; // Column to generate tiles 
        public GameObject column5CTilesPrefab; // Column to generate tiles 
        public GameObject column6CTilesPrefab; // Column to generate tiles 
        public GameObject column7CTilesPrefab; // Column to generate tiles 
        public GameObject btn4letters; // User choose 4 letters per word to play
        public GameObject btn5letters; // User choose 5 letters per word to play
        public GameObject btn6letters; // User choose 6 letters per word to play
        public GameObject btn7letters; // User choose 7 letters per word to play
        public Sprite btOnSwitch; // On Switch image
        public Sprite btOffSwitch; // Off Switch image
        public GameObject tutorialImg; // Show when user wants to see tutorial
        public GameObject txtLetters;
        public GameObject txtLanguage;
        public GameObject txtDarkMode;
        public GameObject txtColorBlindMode;
        public GameObject txtTutorial;
        public GameObject txtShop;
        public GameObject txtRestorePurchase;
        public GameObject txtSocial;
        
        private GameObject _spawnObj; // Use to swap tile
        private GameObject _previousTiles; // Previous column object
        private readonly Words _words = new Words(); // List of words
        private readonly Utility _utility = new Utility();
        private readonly CertainWord _certainWord = new CertainWord(); // Each word
        private WordState[] _states; // State of each character
        private WordData _wordData = new WordData(); // Dictionary of word
        private UserData _userData = new UserData();
        private StringBuilder _keyboard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ"); // All character of keyboard
        private readonly string _initialFilePath = "5lettersToJson.json"; // File path to read initially
        private int _numberOfInputs; // Times of character user input
        private int _numberOfWords; // Number of words user input
        private int _targetLetters = 5; // Number of letters in each word user has to guess
        
        private bool _isEntered; // Is user press on Enter?
        private bool _isInDictionary; // Is that word in Dictionary?
        private bool _isHided; // Is user click to generate new tiles?
        private bool _isDarkMode; // Is user use dark mode?
        private bool _isColorBlindMode; // Is user use color blind mode?
        private string _targetWord; // A Word that user has to guess
        
        private const int NUMBER_OF_ROWS = 6; // Six words to guess
        
        #endregion

        #region Singleton

        private static GameManager _uniqueInstance;
        private static readonly object LockObject = new object();

        public static GameManager Instance
        {
            get
            {
                // instance not exist, then create new one
                if (_uniqueInstance == null)
                    // Avoid multi-threading problem
                    lock (LockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            // create new GameObject, and add GameManager component
                            var singletonObject = new GameObject();
                            _uniqueInstance = singletonObject.AddComponent<GameManager>();
                            singletonObject.name = "Singleton - GameManager";
                        }
                    }

                return _uniqueInstance;
            }
            private set { }
        }

        private void Awake()
        {
            _spawnObj = GenerateTiles(5); // Generate 5 letters of each word initially
            _wordData = _utility.ReadWord(_initialFilePath);
            // Check if there's another instance already exist in scene

            if (_uniqueInstance != null && _uniqueInstance.GetInstanceID() != GetInstanceID())
                Destroy(gameObject);
            else
                // Set instance
                _uniqueInstance = this;
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
            if (_numberOfInputs == 0 || _numberOfInputs % _targetLetters != 0 || _isEntered)
            {
                Texts[_numberOfInputs].text = t.name; // Fill Text
                _numberOfInputs += 1;
                _certainWord.AddCharacterToWord(t.name);
                if (_certainWord.Word.Length == _targetLetters) // If word receive enough characters
                {
                    if (_utility.IsWordInDictionary(_certainWord.Word, _wordData))
                    {
                        _isInDictionary = true;
                        var tempWord = new CertainWord();
                        tempWord.Word = _certainWord.Word;
                        _words.AddWordToList(tempWord);
                        _numberOfWords += 1;
                        _certainWord.Word = ""; // re-init word
                    }
                    else
                    {
                        _isInDictionary = false;
                    }
                }
            }
        }
        
        /// <summary>
        ///     BUG
        /// </summary>
        public void OnEnterButton()
        {
            if (_isInDictionary)
            {
                _isEntered = !_isEntered;
                FillStateToWord();
                FillColorToWord();
                FillColorToKeyboard();
                _states = new WordState[_targetLetters]; // re-init States
            }
            else
            {
                // The input word not in dictionary
                EnablePopup();
                StartCoroutine(WaitThenRestoreTime());
            }
        }
        
        public void OnBackspaceButton()
        {
            if (!_isEntered) // If user don't submit the word yet
            {
                _certainWord.Word = _certainWord.RemoveCharacterFromWord();
                _numberOfInputs -= 1;
                Texts[_numberOfInputs].text = ""; // Set blank text
            }
        }
        
        public void OnSettingButton()
        {
            FillColorToSelectedLetterButton();
            EnableSettingPanel();
        }
        
        #endregion

        #region Settings

        /* Events in Settings */
        
        public void OnExitSettingButton()
        {
            DisablePanels();
        }

        public void OnExitTutorialButton()
        {
            DisableTutorial();
        }
        
        /// <summary>
        ///     User select letters to play
        ///     Exp: Select 4 -> generate 4 letters game play
        /// <param name="t">Transform of button</param>
        /// </summary>
        public void OnSelectLetterButton(Transform t)
        {
            ClearAllKeyboardColor();
            _numberOfWords = 0;
            Texts = new List<Text>(); // Re-init text to fill
            _isHided = !_isHided;
            if (_isHided)
            {
                InitializeObjects(t);
                Common.Log(_targetWord);
            }
            else
            {
                InitializeObjects(t);
                Common.Log(_targetWord);
            }
        }
        
        public void OnDarkMode(Transform t)
        {
            _isDarkMode = !_isDarkMode;
            ChangeTextInButtonDarkMode(btn4letters);
            ChangeTextInButtonDarkMode(btn5letters);
            ChangeTextInButtonDarkMode(btn6letters);
            ChangeTextInButtonDarkMode(btn7letters);
            ChangeTextDarkMode(txtLetters);
            ChangeTextDarkMode(txtLanguage);
            ChangeTextDarkMode(txtDarkMode);
            ChangeTextDarkMode(txtColorBlindMode);
            ChangeTextDarkMode(txtTutorial);
            ChangeTextDarkMode(txtShop);
            ChangeTextDarkMode(txtRestorePurchase);
            ChangeTextDarkMode(txtSocial);
            ChangeSwitchSprite(t, _isDarkMode);
        }

        

        public void OnColorBlindMode(Transform t)
        {
            _isColorBlindMode = !_isColorBlindMode;
            ChangeSwitchSprite(t, _isColorBlindMode);
        }

        public void OnTutorialClick()
        {
            EnableTutorial();
        }
        
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
                return SpawnColumn(numberOfLetters, column4CTilesPrefab);
            }
            if (numberOfLetters == 5)
            {
                return SpawnColumn(numberOfLetters, column5CTilesPrefab);
            }
            if (numberOfLetters == 6)
            {
                return SpawnColumn(numberOfLetters, column6CTilesPrefab);
            }
            if (numberOfLetters == 7)
            {
                return SpawnColumn(numberOfLetters, column7CTilesPrefab);
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
            _spawnObj = Instantiate(prefab, parentSpawn.transform, false);
            _spawnObj.name = "Column";
            for (int i = 1; i <= NUMBER_OF_ROWS; i++)
            {
                for (int j = 1; j <= numberOfLetters; j++)
                {
                    var obj = GameObject.Find("Text" + i + j);
                    Texts.Add(obj.GetComponent<Text>());
                }
            }

            return _spawnObj;
        }

        #endregion
        
        #region UI

        private static readonly Color ON_DARK_MODE_TEXT = new Color(215, 215, 217, 255);
        private static readonly Color OFF_DARK_MODE_TEXT = Color.black;
        private static readonly Color GREEN_COLOR = Color.green; // set to correct position found character
        private static readonly Color YELLOW_COLOR = Color.yellow; // set to incorrect position found character
        private static readonly Color GRAY_COLOR = Color.gray; // set to not found character
        private static readonly Color WHITE_COLOR = Color.white; // set to unused character
        
        /// <summary>
        /// Change text corresponding to dark mode selected
        /// </summary>
        /// <param name="obj">Game object to change text color</param>
        private void ChangeTextInButtonDarkMode(GameObject obj)
        {
            var textToChange = obj.GetComponentInChildren<Text>();

            if (_isDarkMode)
            {
                textToChange.color = ON_DARK_MODE_TEXT;
            }
            else
            {
                textToChange.color = OFF_DARK_MODE_TEXT;
            }
        }

        private void ChangeTextDarkMode(GameObject obj)
        {
            var textToChange = obj.GetComponent<Text>();
            if (_isDarkMode)
            {
                textToChange.color = ON_DARK_MODE_TEXT;
            }
            else
            {
                textToChange.color = OFF_DARK_MODE_TEXT;
            }
        }
        /// <summary>
        ///     Fill state to input word
        /// </summary>
        private void FillStateToWord()
        {
            var inputWord = _words.WordList[_words.WordList.Count - 1].Word;
            _states = _utility.AddStateToWord(inputWord, _targetWord, _targetLetters);
        }

        /// <summary>
        ///     Fill color to input word
        /// </summary>
        private void FillColorToWord()
        {
            var length = _states.Length;
            for (var i = 0; i < length; i++)
            {
                var middleObj = GameObject.Find("Text" + _numberOfWords + (i + 1)); // Text Fill in the middle screen
                var btnMiddleObj = middleObj.transform.parent.gameObject; // Button Object
                FillColorByState(i, btnMiddleObj);
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
            var state = _states[i];
            if (state == WordState.CORRECTPOSITIONFOUND)
                btnMiddleObj.GetComponent<Image>().color = GREEN_COLOR;
            else if (state == WordState.INCORRECTPOSITIONFOUND)
                btnMiddleObj.GetComponent<Image>().color = YELLOW_COLOR;
            else
                btnMiddleObj.GetComponent<Image>().color = GRAY_COLOR;
        }
        
        /// <summary>
        /// Fill green color to selected letter button
        /// Fill white color to unselected letter button
        /// </summary>
        private void FillColorToSelectedLetterButton()
        {
            if (_targetLetters == 4)
            {
                btn4letters.GetComponent<Image>().color = GREEN_COLOR;
                btn5letters.GetComponent<Image>().color = WHITE_COLOR;
                btn6letters.GetComponent<Image>().color = WHITE_COLOR;
                btn7letters.GetComponent<Image>().color = WHITE_COLOR;
            }

            if (_targetLetters == 5)
            {
                btn4letters.GetComponent<Image>().color = WHITE_COLOR;
                btn5letters.GetComponent<Image>().color = GREEN_COLOR;
                btn6letters.GetComponent<Image>().color = WHITE_COLOR;
                btn7letters.GetComponent<Image>().color = WHITE_COLOR;
            }

            if (_targetLetters == 6)
            {
                btn4letters.GetComponent<Image>().color = WHITE_COLOR;
                btn5letters.GetComponent<Image>().color = WHITE_COLOR;
                btn6letters.GetComponent<Image>().color = GREEN_COLOR;
                btn7letters.GetComponent<Image>().color = WHITE_COLOR;
            }

            if (_targetLetters == 7)
            {
                btn4letters.GetComponent<Image>().color = WHITE_COLOR;
                btn5letters.GetComponent<Image>().color = WHITE_COLOR;
                btn6letters.GetComponent<Image>().color = WHITE_COLOR;
                btn7letters.GetComponent<Image>().color = GREEN_COLOR;
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
            var listOfInputCharacter = GetInputCharacter();
            for (var i = 0; i < listOfInputCharacter.Count; i++)
            {
                var bottomObj = GameObject.Find(listOfInputCharacter[i] + "txt");
                var btnBottomObj = bottomObj.transform.parent.gameObject;
                FillColorByState(i, btnBottomObj);
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
                t.transform.gameObject.GetComponent<Image>().sprite = btOnSwitch;
            }
            else
            {
                t.transform.gameObject.GetComponent<Image>().sprite = btOffSwitch;
            }
        }
        
        /// <summary>
        ///     Get word base on input characters
        /// </summary>
        /// <returns>Word base on input characters</returns>
        private List<string> GetInputCharacter()
        {
            var list = new List<string>();
            for (var i = 0; i < _targetLetters; i++)
                list.Add(_words.WordList[_words.WordList.Count - 1].Word[i].ToString());
            return list;
        }

        private void DisablePanels()
        {
            settingPanel.SetActive(false);
            tutorialImg.SetActive(false);
        }

        private void EnableSettingPanel()
        {
            settingPanel.SetActive(true);
        }

        private void DisablePopUp()
        {
            warningPopUp.SetActive(false);
        }

        private void EnablePopup()
        {
            warningPopUp.SetActive(true);
        }

        private void DisableTutorial()
        {
            tutorialImg.SetActive(false);
        }
        private void EnableTutorial()
        {
            tutorialImg.SetActive(true);
        }


        private IEnumerator WaitThenRestoreTime()
        {
            yield return new WaitForSecondsRealtime(popUpTime);
            DisablePopUp();
        }

        #endregion

        #region RE-INIT

        private const string FILE_POSTFIX = "lettersToJson.json";
        /// <summary>
        /// Clear all keyboard color
        /// </summary>
        private void ClearAllKeyboardColor()
        {
            for (int i = 0; i < _keyboard.Length; i++)
            {
                var bottomObj = GameObject.Find(_keyboard[i].ToString());
                bottomObj.GetComponent<Image>().color = Color.white;
            }
        }

        /// <summary>
        /// Initialize corresponding dictionary
        /// Hide previous tiles
        /// Create new random word
        /// </summary>
        /// <param name="t">Transform of button</param>
        private void InitializeObjects(Transform t)
        {
            _spawnObj.SetActive(false); // Hide previous tiles
            var filePrefix = t.GetChild(0).gameObject.GetComponent<Text>().text; // Get number of letters
            _targetLetters = Int32.Parse(filePrefix); // Reset target letters of each word
            _wordData = _utility.ReadWord(filePrefix + FILE_POSTFIX); // Initialize corresponding dictionary 
            _words.WordList = new List<CertainWord>(); // Re-init list
            _numberOfInputs = 0; // Re-init input click
            _utility.GetRandomWordInDictionary(ref _targetWord, _wordData);
            _previousTiles = GenerateTiles(_targetLetters);
            DisablePanels();
        }
        #endregion
        
        #region WIN GAME & LOSE GAME

        /// <summary>
        ///     Check từ user nhập chính xác với từ trong Dictionary -> ĐÚng thì chạy win game
        ///     Sai thi hiển thị UI Lose game
        ///     Input: từ user nhập (đã lưu trong WordList)
        ///     -> Output: UI Win Game, gồm có: Replay button, Text win game
        /// </summary>
        private void WinGame()
        {
        }

        private void LoseGame()
        {
        }

        /// <summary>
        ///     Khi user nhấn vào setting, tắt setting sẽ quay lại trạng thái chơi
        ///     Sau khi user mở lại trò chơi, sẽ có UI hiện lên và hỏi user có muốn quay lại scene đang chơi không?
        ///     Nếu có thì sẽ load lại trạng thái cuối cùng, không thì sẽ reset scence và chơi lại
        /// </summary>
        private void OnResumeButton()
        {
        }

        /// <summary>
        ///     Khi user nhấn vào button -> hiển thị Confirm UI. Đồng ý thì sẽ load lại scene, không thì tắt UI
        /// </summary>
        private void OnReplayButton()
        {
        }

        #endregion
    }
}