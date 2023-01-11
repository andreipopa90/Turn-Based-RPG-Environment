using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.MoveSelectionUI
{
    public class StarterPicker : MonoBehaviour
    {

        public Image imageComponent;
        private Color32 _imageColor;
        private bool Selected { get; set; }

        private readonly Dictionary<string, string> _nameMatcher = new()
        {
            {"Pokemon 1", "Blaziken"},
            {"Pokemon 2", "Sceptile"},
            {"Pokemon 3", "Swampert"}
        };

        [FormerlySerializedAs("ChosenIndicator")]
        public Text chosenIndicator;

        private GameStateStorage GameState { get; set; }

        private void Start()
        {
            GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            Selected = false;
        }

        public void OnMouseOver()
        {
            if (!string.IsNullOrEmpty(GameState.StarterPokemon)) return;
            _imageColor = new Color32(255, 255, 255, 255);
            imageComponent.color = _imageColor;
        }

        public void OnMouseRemove()
        {
            if (Selected) return;
            _imageColor = new Color32(0, 0, 0, 255);
            imageComponent.color = _imageColor;
        }

        public void OnMouseClick()
        {
            if (string.IsNullOrEmpty(GameState.StarterPokemon))
            {
                OnMouseOver();
                Selected = !Selected;
                GameState.StarterPokemon = _nameMatcher[name];
                UpdateText(GameState.StarterPokemon);
                var starter = GameState.BaseStats.Find(b => b.Name.Equals(GameState.StarterPokemon));
                GameState.StarterStats = starter;
                GameState.SelectedMoves = GameState.AllMoves
                    .Where(m => StarterTable.StartersList[starter.Name].Contains(m.KeyName)).ToList();

            }
            else if (_nameMatcher[name].Equals(GameState.StarterPokemon))
            {
                Selected = !Selected;
                GameState.StarterPokemon = string.Empty;
                UpdateText(GameState.StarterPokemon);
                GameState.StarterStats = new BaseStat();
                GameState.SelectedMoves.Clear();
            }
        }

        private void UpdateText(string starterName)
        {
            chosenIndicator.text = "You chose: " + starterName;
        }
        
        // public void OnPressLockIn()
        // {
        //     if (gameState.SelectedMoves.Count == 4 && !string.IsNullOrEmpty(gameState.StarterPokemon))
        //     {
        //         SceneManager.LoadScene("BattleScene");
        //     }
        // }
    }
}