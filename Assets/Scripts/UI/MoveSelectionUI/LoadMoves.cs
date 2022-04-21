using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadMoves : MonoBehaviour
{

    public GameObject MovesPanel;
    public Button MoveButtonPrefab;
    public GameObject GameState;
    public Text SelectedMovesIndicator;
    List<Move> Moves;

    // Start is called before the first frame update
    void Start()
    {
        JSONReader Reader = new();
        Moves = Reader.ReadMovesJSON();
        foreach (Move Move in Moves)
        {
            if (Move.BasePower <= 50 && (Move.Type.Equals("Grass") || Move.Type.Equals("Fire") || Move.Type.Equals("Water") || Move.Type.Equals("Normal")) && !Move.Name.Contains("G-Max"))
            {
                Button Button = Instantiate(MoveButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                Button.transform.SetParent(MovesPanel.transform, false);
                Button.GetComponent<RectTransform>().localScale = new Vector3(2.5f, 1f, 0f);
                Button.GetComponentInChildren<Text>().text = Move.Name;
                Button.name = Move.Name;
                Button.onClick.AddListener(OnButtonClick);
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
            if (GameState.GetComponent<GameStateStorage>().SelectedMoves.Count >= 8)
            {
                print("Cannot select more than 8 moves");
            }
            else
            {
                ButtonPressed.GetComponent<Image>().color = Green;
                GameState.GetComponent<GameStateStorage>().SelectedMoves.Add(Move);
                SelectedMovesIndicator.text = "Selected Moves: " + GameState.GetComponent<GameStateStorage>().SelectedMoves.Count + "/8";
            }
        } else if (ButtonColor.Equals(Green))
        {
            ButtonPressed.GetComponent<Image>().color = White;
            GameState.GetComponent<GameStateStorage>().SelectedMoves.Remove(Move);
            SelectedMovesIndicator.text = "Selected Moves: " + GameState.GetComponent<GameStateStorage>().SelectedMoves.Count + "/8";
        }
        
    }

    public void OnPressLockIn()
    {
        if (GameState.GetComponent<GameStateStorage>().SelectedMoves.Count == 8)
        {
            SceneManager.LoadScene("BattleScene");
        }
    }
}
