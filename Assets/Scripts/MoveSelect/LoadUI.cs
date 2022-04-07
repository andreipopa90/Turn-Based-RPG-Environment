using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadUI : MonoBehaviour
{

    public GameObject MovesPanel;
    public Button MoveButtonPrefab;
    public GameObject MoveSelection;
    List<Move> Moves;

    // Start is called before the first frame update
    void Start()
    {
        JSONReader Reader = new();
        Moves = Reader.ReadMovesJSON();
        foreach (Move Move in Moves)
        {
            if (Move.basePower <= 50 && (Move.type.Equals("Grass") || Move.type.Equals("Fire") || Move.type.Equals("Water") || Move.type.Equals("Normal")) && !Move.name.Contains("G-Max"))
            {
                Button Button = Instantiate(MoveButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                Button.transform.SetParent(MovesPanel.transform, false);
                Button.GetComponent<RectTransform>().localScale = new Vector3(2.5f, 1f, 0f);
                Button.GetComponentInChildren<Text>().text = Move.name;
                Button.name = Move.name;
                Button.onClick.AddListener(OnButtonClick);
            }
        }
    }

    public void OnButtonClick()
    {
        GameObject ButtonPressed = EventSystem.current.currentSelectedGameObject;
        Color32 ButtonColor = ButtonPressed.GetComponent<Image>().color;
        Move Move = Moves.Find(x => x.name.Equals(ButtonPressed.name));
        if (ButtonColor.r == 255 && ButtonColor.g == 255 && ButtonColor.b == 255)
        {
            if (MoveSelection.GetComponent<MoveSelection>().SelectedMoves.Count >= 8)
            {
                print("Cannot select more than 8 moves");
            }
            else
            {
                ButtonPressed.GetComponent<Image>().color = new Color(0, 255, 0);
                MoveSelection.GetComponent<MoveSelection>().SelectedMoves.Add(Move);
            }
        } else if (ButtonColor.r == 0 && ButtonColor.g == 255 && ButtonColor.b == 0)
        {
            ButtonPressed.GetComponent<Image>().color = new Color(255, 255, 255);
            MoveSelection.GetComponent<MoveSelection>().SelectedMoves.Remove(Move);
        }
        
    }
}
