using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        bool PressedOwn = false;
        bool PressedNew = false;
        string OwnMove = "";
        string NewMove = "";

        private void Start()
        {
            GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            AddMoveButtons();
            AddNewMoveButtons();
        }

        Button InstantiateButton(Vector2 Position, Vector3 Size, string Name, GameObject Parent)
        {
            Button ButtonInstance = Instantiate(AbilityButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            ButtonInstance.transform.SetParent(Parent.transform, false);
            ButtonInstance.name = Name;
            ButtonInstance.GetComponent<RectTransform>().anchoredPosition = Position;
            ButtonInstance.GetComponent<RectTransform>().localScale = Size;

            ButtonInstance.GetComponentInChildren<Text>().text = Name;
            return ButtonInstance;
        }

        void AddMoveButtons()
        {
            for(int i = 0; i < 8; i++)
            {
                int x = (i < 4) ? -200 : 200;
                int y = (i < 4) ? 150 - i * 100 : 150 - (i - 4) * 100;
                Button ButtonInstance = InstantiateButton(new Vector2(x, y), new Vector3(1.5f, 1.5f, 0f), GameState.SelectedMoves[i].Name, MovesPanel);
                ButtonInstance.onClick.AddListener(OnClickOwnMove);
            }
        }

        void AddNewMoveButtons()
        {
            for (int i = 0; i < 3; i++)
            {
                Button ButtonInstance = InstantiateButton(new Vector2(-250 + i * 250, 0), new Vector3(1.5f, 2.5f, 0f), GameState.AllMoves[i].Name, NewMovesPanel);
                ButtonInstance.onClick.AddListener(OnClickNewMove);
            }
        }

        void OnClickOwnMove()
        {
            GameObject ButtonPressed = EventSystem.current.currentSelectedGameObject;
            if (!PressedOwn || ButtonPressed.GetComponentInChildren<Text>().text.Equals(OwnMove))
            {
                if (ButtonPressed.GetComponent<Image>().color.Equals(White))
                {
                    PressedOwn = true;
                    ButtonPressed.GetComponent<Image>().color = Red;
                    OwnMove = ButtonPressed.GetComponentInChildren<Text>().text;
                }
                else
                {
                    PressedOwn = false;
                    ButtonPressed.GetComponent<Image>().color = White;
                    OwnMove = "";
                }
            }
        }

        void OnClickNewMove()
        {
            GameObject ButtonPressed = EventSystem.current.currentSelectedGameObject;
            if (!PressedNew || ButtonPressed.GetComponentInChildren<Text>().text.Equals(NewMove))
            {
                if (ButtonPressed.GetComponent<Image>().color.Equals(White))
                {
                    PressedNew = true;
                    ButtonPressed.GetComponent<Image>().color = Green;
                    NewMove = ButtonPressed.GetComponentInChildren<Text>().text;
                }
                else
                {
                    PressedNew = false;
                    ButtonPressed.GetComponent<Image>().color = White;
                    NewMove = "";
                }
            }
        }

        public void OnClickStart()
        {
            if (!(PressedNew ^ PressedOwn))
            {
                if (!OwnMove.Equals("") && !NewMove.Equals(""))
                {
                    GameState.SelectedMoves.Remove(GameState.SelectedMoves.Find(x => x.Name.Equals(OwnMove)));
                    GameState.SelectedMoves.Add(GameState.AllMoves.Find(x => x.Name.Equals(NewMove)));
                }

                SceneManager.LoadScene("BattleScene");
            
            }
        }

    }
}

