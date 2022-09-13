using System.Linq;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

namespace UI.TransitionUI
{
    class LoadTransitionUI : MonoBehaviour
    {
        private GameStateStorage GameState;
        public Button AbilityButtonPrefab;
        public GameObject MovesPanel;
        public GameObject NewMovesPanel;
        Color32 White = new(255, 255, 255, 255);
        Color32 Green = new(0, 255, 0, 255);
        Color32 Red = new(255, 0, 0, 255);
        private bool _pressedOwn;
        private bool _pressedNew;
        private string _ownMove = string.Empty;
        private string _newMove = string.Empty;

        private void Start()
        {
            GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            AddMoveButtons();
            AddNewMoveButtons();
        }

        private Button InstantiateButton(Vector2 position, Vector3 size, string buttonName, GameObject parent)
        {
            var buttonInstance = Instantiate(AbilityButtonPrefab, 
                new Vector3(0, 0, 0), Quaternion.identity);
            buttonInstance.transform.SetParent(parent.transform, false);
            buttonInstance.name = buttonName;
            buttonInstance.GetComponent<RectTransform>().anchoredPosition = position;
            buttonInstance.GetComponent<RectTransform>().localScale = size;

            buttonInstance.GetComponentInChildren<Text>().text = buttonName;
            return buttonInstance;
        }

        private void AddMoveButtons()
        {
            for(var i = 0; i < 8; i++)
            {
                var x = i < 4 ? -200 : 200;
                var y = i < 4 ? 150 - i * 100 : 150 - (i - 4) * 100;
                var buttonInstance = InstantiateButton(new Vector2(x, y), 
                    new Vector3(1.5f, 1.5f, 0f), GameState.SelectedMoves[i].Name, MovesPanel);
                buttonInstance.onClick.AddListener(OnClickOwnMove);
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
            var buttonInstance = InstantiateButton(new Vector2(-250, 0), 
                new Vector3(1.5f, 2.5f, 0f), buffs[randomIndex].Name, NewMovesPanel);
            buttonInstance.onClick.AddListener(OnClickNewMove);
            
            randomIndex = new Random().Next(0, debuffs.Count);
            buttonInstance = InstantiateButton(new Vector2(0, 0), 
                new Vector3(1.5f, 2.5f, 0f), debuffs[randomIndex].Name, NewMovesPanel);
            buttonInstance.onClick.AddListener(OnClickNewMove);
            
            randomIndex = new Random().Next(0, newMoves.Count);
            buttonInstance = InstantiateButton(new Vector2(250, 0), 
                new Vector3(1.5f, 2.5f, 0f), newMoves[randomIndex].Name, NewMovesPanel);
            buttonInstance.onClick.AddListener(OnClickNewMove);
        }

        void OnClickOwnMove()
        {
            var buttonPressed = EventSystem.current.currentSelectedGameObject;
            if (_pressedOwn && !buttonPressed.GetComponentInChildren<Text>().text.Equals(_ownMove)) return;
            if (buttonPressed.GetComponent<Image>().color.Equals(White))
            {
                _pressedOwn = true;
                buttonPressed.GetComponent<Image>().color = Red;
                _ownMove = buttonPressed.GetComponentInChildren<Text>().text;
            }
            else
            {
                _pressedOwn = false;
                buttonPressed.GetComponent<Image>().color = White;
                _ownMove = "";
            }
        }

        void OnClickNewMove()
        {
            var buttonPressed = EventSystem.current.currentSelectedGameObject;
            if (_pressedNew && !buttonPressed.GetComponentInChildren<Text>().text.Equals(_newMove)) return;
            if (buttonPressed.GetComponent<Image>().color.Equals(White))
            {
                _pressedNew = true;
                buttonPressed.GetComponent<Image>().color = Green;
                _newMove = buttonPressed.GetComponentInChildren<Text>().text;
            }
            else
            {
                _pressedNew = false;
                buttonPressed.GetComponent<Image>().color = White;
                _newMove = "";
            }
        }

        public void OnClickStart()
        {
            if (_pressedNew ^ _pressedOwn) return;
            
            if (!string.IsNullOrEmpty(_ownMove) && !string.IsNullOrEmpty(_newMove))
            {
                GameState.SelectedMoves.Remove(GameState.SelectedMoves.Find(x => x.Name.Equals(_ownMove)));
                GameState.SelectedMoves.Add(GameState.AllMoves.Find(x => x.Name.Equals(_newMove)));
            }

            SceneManager.LoadScene("BattleScene");
        }

    }
}

