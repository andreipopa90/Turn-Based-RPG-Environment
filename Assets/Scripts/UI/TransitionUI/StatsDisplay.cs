using System;
using Model;
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

        private GameStateStorage _gameState;

        private void Start()
        {
            _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            HP.text = _gameState.StarterStats.Hp.ToString();
            ATK.text = _gameState.StarterStats.Atk.ToString();
            DEF.text = _gameState.StarterStats.Def.ToString();
            SPA.text = _gameState.StarterStats.Spa.ToString();
            SPD.text = _gameState.StarterStats.Spd.ToString();
            SPE.text = _gameState.StarterStats.Spe.ToString();
        }
    }
}