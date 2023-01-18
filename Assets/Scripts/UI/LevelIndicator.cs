using System;
using Model;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LevelIndicator : MonoBehaviour
    {
        [field: SerializeField] public TextMeshProUGUI Text { get; set; }
        private GameStateStorage GameStateStorage { get; set; }

        private void Start()
        {
            GameStateStorage = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            SetLevel(GameStateStorage.CurrentLevel);
        }

        private void SetLevel(int level)
        {
            Text.text = "Level " + level;
        }
        
    }
}