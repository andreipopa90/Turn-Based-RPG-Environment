using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.MoveSelectionUI
{
    public class MovesPicker : MonoBehaviour
    {

        [FormerlySerializedAs("MovesPanel")] public GameObject movesPanel;
        [FormerlySerializedAs("MoveButtonPrefab")] public Button moveButtonPrefab;
        [FormerlySerializedAs("GameState")] public GameStateStorage gameState;
        [FormerlySerializedAs("SelectedMovesIndicator")] public Text selectedMovesIndicator;
        private List<Move> _moves;
        private List<Button> ButtonsList { get; set; }
        
        private readonly Dictionary<string, string> _nameMatcher = new()
        {
            {"Pokemon 1", "Blaziken"},
            {"Pokemon 2", "Sceptile"},
            {"Pokemon 3", "Swampert"}
        };

        // Start is called before the first frame update
        private void Start()
        {
            ButtonsList = new List<Button>();
        }

        public void AddMoves(GameObject trigger)
        {
            if (string.IsNullOrEmpty(gameState.StarterPokemon))
            {
                ClearMoveSelectionList();
            }
            else if (gameState.StarterPokemon.Equals(_nameMatcher[trigger.name]))
            {
                _moves = gameState.AllMoves;
                var startMoves = gameState.StartMoves.Find(sm => sm.Name.Equals(gameState.StarterPokemon.ToLower())).LearnSet;
                var moves = _moves.Where(m => startMoves.Contains(m.KeyName) && m.BasePower <= 50 &&
                                              (m.MoveType.Equals("Fire") || m.MoveType.Equals("Grass") ||
                                               m.MoveType.Equals("Water") || m.MoveType.Equals("Normal"))).ToList();
                foreach (var move in moves)
                {
                    var button = Instantiate(moveButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    button.transform.SetParent(movesPanel.transform, false);
                    button.GetComponent<RectTransform>().localScale = new Vector3(2f, 1f, 0f);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = move.Name;
                    button.name = move.Name;
                    button.onClick.AddListener(OnButtonClick);
                    ButtonsList.Add(button);
                }
            }
        }

        private void ClearMoveSelectionList()
        {
            foreach (var t in ButtonsList)
            {
                Destroy(t.gameObject);
            }

            ButtonsList.Clear();
            gameState.SelectedMoves.Clear();
            selectedMovesIndicator.text = "Selected Moves: " + gameState.SelectedMoves.Count + "/4";
        }

        private void OnButtonClick()
        {
            var buttonPressed = EventSystem.current.currentSelectedGameObject;
            Color32 buttonColor = buttonPressed.GetComponent<Image>().color;
            var move = _moves.Find(x => x.Name.Equals(buttonPressed.name));
            Color32 white = new(255, 255, 255, 255);
            Color32 green = new(0, 255, 0, 255);
            if (buttonColor.Equals(white))
            {
                if (gameState.SelectedMoves.Count >= 4)
                {
                    print("Cannot select more than 4 moves");
                }
                else
                {
                    buttonPressed.GetComponent<Image>().color = green;
                    gameState.SelectedMoves.Add(move);
                    selectedMovesIndicator.text = "Selected Moves: " + gameState.SelectedMoves.Count + "/4";
                }
            } else if (buttonColor.Equals(green))
            {
                buttonPressed.GetComponent<Image>().color = white;
                gameState.SelectedMoves.Remove(move);
                selectedMovesIndicator.text = "Selected Moves: " + gameState.SelectedMoves.Count + "/4";
            }
        
        }

        public void OnPressLockIn()
        {
            if (gameState.SelectedMoves.Count == 4 && !string.IsNullOrEmpty(gameState.StarterPokemon))
            {
                SceneManager.LoadScene("BattleScene");
            }
        }
    }
}
