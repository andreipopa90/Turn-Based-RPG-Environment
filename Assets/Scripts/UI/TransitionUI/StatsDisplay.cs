using System;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TransitionUI
{
    public class StatsDisplay : MonoBehaviour
    {
        public Text HP;
        public Text ATK;
        public Text DEF;
        public Text SPA;
        public Text SPD;
        public Text SPE;
        public TextMeshProUGUI TotalStats;

        private GameStateStorage _gameState;

        private void Awake()
        {
            _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            HP.text = _gameState.StarterStats.Hp.ToString();
            ATK.text = _gameState.StarterStats.Atk.ToString();
            DEF.text = _gameState.StarterStats.Def.ToString();
            SPA.text = _gameState.StarterStats.Spa.ToString();
            SPD.text = _gameState.StarterStats.Spd.ToString();
            SPE.text = _gameState.StarterStats.Spe.ToString();
        }

        public void UpdateTotalStats(string newText)
        {
            TotalStats.text = newText;
        }
    }
}