using System.Collections.Generic;
using System.Linq;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

namespace UI.TransitionUI
{
    internal class TransitionUI : MonoBehaviour
    {
        private GameStateStorage GameState;
        public Button AbilityButtonPrefab;
        public GameObject MovesPanel;
        public GameObject NewMovesPanel;
        public GameObject PanelOne;
        public GameObject PanelTwoOne;
        public GameObject PanelTwoTwo;
        public GameObject PanelThree;
        public Button[] ownMovesButtons;
        public Button[] newMovesButtons;
        private readonly Color32 _white = new(255, 255, 255, 255);
        private readonly Color32 _green = new(0, 255, 0, 255);
        private readonly Color32 _red = new(255, 0, 0, 255);
        private bool _pressedOwn;
        private bool _pressedNew;
        private string _ownMove = string.Empty;
        private string _newMove = string.Empty;
        private bool _pressedOwnType;
        private bool _pressedNewType;
        private string _ownType = string.Empty;
        private string _newType = string.Empty;
        public Button[] OwnTypes;
        public Button[] NewTypesOne;
        public Button[] NewTypesTwo;
        

        private void Start()
        {
            GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            PanelOne.SetActive(true);
            AddMoveButtons();
            AddNewMoveButtons();
        }

        private void AddMoveButtons()
        {
            for(var i = 0; i < ownMovesButtons.Length; i++)
            {
                ownMovesButtons[i].onClick.AddListener(OnClickOwnMove);
                ownMovesButtons[i].name = GameState.SelectedMoves[i].Name;
                ownMovesButtons[i].transform.Find("Move Name").GetComponent<TextMeshProUGUI>().text =
                    GameState.SelectedMoves[i].Name;
            }
        }

        private void AddNewMoveButtons()
        {
            var buffs = GameState.AllMoves.Where(e => e.Target.Equals("self")).ToList();
            buffs.RemoveAll(e => GameState.SelectedMoves.Contains(e));
            var debuffs = GameState.AllMoves.Where(e => !e.Target.Equals("self") && e.Category.Equals("Status")).ToList();
            debuffs.RemoveAll(e => GameState.SelectedMoves.Contains(e));
            var newMoves = GameState.AllMoves.Where(e => !e.Target.Equals("self") && !e.Category.Equals("Status")).ToList();
            newMoves.RemoveAll(e => GameState.SelectedMoves.Contains(e));

            var randomIndex = new Random().Next(0, buffs.Count);
            newMovesButtons[0].name = buffs[randomIndex].Name;
            newMovesButtons[0].transform.Find("Move Name").GetComponent<TextMeshProUGUI>().text =
                buffs[randomIndex].Name;
            newMovesButtons[0].onClick.AddListener(OnClickNewMove);
            
            randomIndex = new Random().Next(0, debuffs.Count);
            newMovesButtons[1].name = debuffs[randomIndex].Name;
            newMovesButtons[1].transform.Find("Move Name").GetComponent<TextMeshProUGUI>().text =
                debuffs[randomIndex].Name;
            newMovesButtons[1].onClick.AddListener(OnClickNewMove);
            
            randomIndex = new Random().Next(0, newMoves.Count);
            newMovesButtons[2].name = newMoves[randomIndex].Name;
            newMovesButtons[2].transform.Find("Move Name").GetComponent<TextMeshProUGUI>().text =
                newMoves[randomIndex].Name;
            newMovesButtons[2].onClick.AddListener(OnClickNewMove);
        }

        private void OnClickOwnMove()
        {
            var buttonPressed = EventSystem.current.currentSelectedGameObject;
            if (_pressedOwn && !buttonPressed.gameObject.transform.Find("Move Name")
                    .GetComponent<TextMeshProUGUI>().text.Equals(_ownMove)) return;
            if (buttonPressed.GetComponent<Image>().color.Equals(_white))
            {
                _pressedOwn = true;
                buttonPressed.GetComponent<Image>().color = _red;
                _ownMove = buttonPressed.gameObject.transform.Find("Move Name").GetComponent<TextMeshProUGUI>().text;
            }
            else
            {
                _pressedOwn = false;
                buttonPressed.GetComponent<Image>().color = _white;
                _ownMove = string.Empty;
            }
        }

        private void OnClickNewMove()
        {
            var buttonPressed = EventSystem.current.currentSelectedGameObject;
            if (_pressedNew && !buttonPressed.gameObject.transform.Find("Move Name")
                    .GetComponent<TextMeshProUGUI>().text.Equals(_newMove)) return;
            if (buttonPressed.GetComponent<Image>().color.Equals(_white))
            {
                _pressedNew = true;
                buttonPressed.GetComponent<Image>().color = _green;
                _newMove = buttonPressed.gameObject.transform.Find("Move Name").GetComponent<TextMeshProUGUI>().text;
            }
            else
            {
                _pressedNew = false;
                buttonPressed.GetComponent<Image>().color = _white;
                _newMove = string.Empty;
            }
        }

        public void OnClickStart()
        {
            switch (GameState.LostCurrentLevel)
            {
                case false:
                    GameState.CreateLevel();
                    break;
                case true when GameState.Dynamic:
                    GameState.ReduceDifficulty();
                    break;
            }

            SceneManager.LoadScene("BattleScene");
        }

        public void OnClickNextPanelOne()
        {
            if (_pressedNew && !_pressedOwn || !_pressedNew && _pressedOwn) return;
            
            if (!string.IsNullOrEmpty(_ownMove) && !string.IsNullOrEmpty(_newMove))
            {
                GameState.SelectedMoves.Remove(GameState.SelectedMoves.Find(x => x.Name.Equals(_ownMove)));
                GameState.SelectedMoves.Add(GameState.AllMoves.Find(x => x.Name.Equals(_newMove)));
            }
            
            PanelOne.SetActive(false);
            if (GameState.StarterStats.Types.Count == 2)
            {
                PanelTwoOne.SetActive(true);
                SetOwnTypeButtons();
                SetNewTypeButtons();
            }
            else
            {
                PanelTwoTwo.SetActive(true);
                SetNewTypeButtons();
            }
        }

        private void SetOwnTypeButtons()
        {
            var types = GameState.StarterStats.Types;
            for (var i = 0; i < OwnTypes.Length; i++)
            {
                OwnTypes[i].name = types[i];
                OwnTypes[i].onClick.AddListener(OnClickOwnType);
                OwnTypes[i].GetComponentInChildren<TextMeshProUGUI>().text = types[i];
            }
        }

        private void SetNewTypeButtons()
        {
            var newTypes = GameState.TypeChart.Where(t => 
                !GameState.StarterStats.Types.Contains(t.Name)).ToList();
            var rng = new Random();
            newTypes = newTypes.OrderBy(t => rng.Next() * 1000).ToList();
            for (var i = 0; i < NewTypesOne.Length; i++)
            {
                NewTypesOne[i].name = newTypes[i].Name;
                NewTypesOne[i].onClick.AddListener(OnClickNewType);
                NewTypesOne[i].GetComponentInChildren<TextMeshProUGUI>().text = newTypes[i].Name;
            }
            for (var i = 0; i < NewTypesTwo.Length; i++)
            {
                NewTypesTwo[i].name = newTypes[i].Name;
                NewTypesTwo[i].onClick.AddListener(OnClickNewType);
                NewTypesTwo[i].GetComponentInChildren<TextMeshProUGUI>().text = newTypes[i].Name;
            }
        }

        public void OnClickNextPanelTwo()
        {
            if (PanelTwoTwo.activeSelf && _pressedNewType)
            {
                GameState.StarterStats.Types.Add(_newType);
            }
            else if (PanelTwoOne.activeSelf)
            {
                switch (_pressedNewType)
                {
                    case true when !_pressedOwnType:
                    case false when _pressedOwnType:
                        return;
                    case true when _pressedOwnType:
                        GameState.StarterStats.Types.Remove(_ownType);
                        GameState.StarterStats.Types.Add(_newType);
                        break;
                }
            }
            PanelTwoOne.SetActive(false);
            PanelTwoTwo.SetActive(false);
            PanelThree.SetActive(true);
        }

        private void OnClickNewType()
        {
            var buttonPressed = EventSystem.current.currentSelectedGameObject;

            if (_pressedNewType && !buttonPressed.gameObject.name.Equals(_newType)) return;
            
            if (buttonPressed.GetComponent<Image>().color.Equals(_white))
            {
                _pressedNewType = true;
                buttonPressed.GetComponent<Image>().color = _green;
                _newType = buttonPressed.gameObject.name;
            }
            else
            {
                _pressedNewType = false;
                buttonPressed.GetComponent<Image>().color = _white;
                _newType = string.Empty;
            }
        }

        private void OnClickOwnType()
        {
            var buttonPressed = EventSystem.current.currentSelectedGameObject;

            if (_pressedOwnType && !buttonPressed.gameObject.name.Equals(_ownType)) return;
            
            if (buttonPressed.GetComponent<Image>().color.Equals(_white))
            {
                _pressedOwnType = true;
                buttonPressed.GetComponent<Image>().color = _red;
                _ownType = buttonPressed.gameObject.name;
            }
            else
            {
                _pressedOwnType = false;
                buttonPressed.GetComponent<Image>().color = _white;
                _ownType = string.Empty;
            }
        }

        public void OnClickStop()
        {
            GameState.GameStatistics.PrintStatistics();
            #if UNITY_STANDALONE
                Application.Quit();
            #endif
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

    }
}

