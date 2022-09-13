using System.Collections.Generic;
using JsonParser;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.MoveSelectionUI
{
    public class LoadMoves : MonoBehaviour
    {

        public GameObject MovesPanel;
        public Button MoveButtonPrefab;
        public GameObject GameState;
        public Text SelectedMovesIndicator;
        List<Move> Moves;

        // Start is called before the first frame update
        private void Start()
        {
            JSONReader reader = new();
            Moves = reader.ReadMovesJson();
            foreach (var move in Moves)
            {
                if (move.BasePower <= 50 && (move.MoveType.Equals("Grass") || 
                                             move.MoveType.Equals("Fire") || 
                                             move.MoveType.Equals("Water") || 
                                             move.MoveType.Equals("Normal")) && !move.Name.Contains("G-Max"))
                {
                    var button = Instantiate(MoveButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    button.transform.SetParent(MovesPanel.transform, false);
                    button.GetComponent<RectTransform>().localScale = new Vector3(2.5f, 1f, 0f);
                    button.GetComponentInChildren<Text>().text = move.Name;
                    button.name = move.Name;
                    button.onClick.AddListener(OnButtonClick);
                }
            }
        }

        public void OnButtonClick()
        {
            GameObject ButtonPressed = EventSystem.current.currentSelectedGameObject;
            Color32 ButtonColor = ButtonPressed.GetComponent<Image>().color;
            Move Move = Moves.Find(x => x.Name.Equals(ButtonPressed.name));
            Color32 White = new(255, 255, 255, 255);
            Color32 Green = new(0, 255, 0, 255);
            if (ButtonColor.Equals(White))
            {
                if (GameState.GetComponent<GameStateStorage>().SelectedMoves.Count >= 6)
                {
                    print("Cannot select more than 6 moves");
                }
                else
                {
                    ButtonPressed.GetComponent<Image>().color = Green;
                    GameState.GetComponent<GameStateStorage>().SelectedMoves.Add(Move);
                    SelectedMovesIndicator.text = "Selected Moves: " + GameState.GetComponent<GameStateStorage>().SelectedMoves.Count + "/6";
                }
            } else if (ButtonColor.Equals(Green))
            {
                ButtonPressed.GetComponent<Image>().color = White;
                GameState.GetComponent<GameStateStorage>().SelectedMoves.Remove(Move);
                SelectedMovesIndicator.text = "Selected Moves: " + GameState.GetComponent<GameStateStorage>().SelectedMoves.Count + "/6";
            }
        
        }

        public void OnPressLockIn()
        {
            if (GameState.GetComponent<GameStateStorage>().SelectedMoves.Count == 6)
            {
                SceneManager.LoadScene("BattleScene");
            }
        }
    }
}
