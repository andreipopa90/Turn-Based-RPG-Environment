using System.Linq;
using System.Reflection;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TransitionUI
{
    public class StatChanger : MonoBehaviour
    {
        private GameStateStorage GameStateStorage { get; set; }
        private int CurrentStatsSum { get; set; }
        private int NewStatSum { get; set; }

        public StatsDisplay display;
        private void Start()
        {
            GameStateStorage = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            CurrentStatsSum = GameStateStorage.StarterStats.Atk +
                               GameStateStorage.StarterStats.Spa +
                               GameStateStorage.StarterStats.Spd +
                               GameStateStorage.StarterStats.Spe +
                               GameStateStorage.StarterStats.Def +
                               GameStateStorage.StarterStats.Hp;
            NewStatSum = CurrentStatsSum;
        }
        public void OnPlusClick(Text stat)
        {
            if (NewStatSum + 1 > CurrentStatsSum) return;
            var property = GameStateStorage.StarterStats.GetType().GetProperties().ToList()
                .Find(p => p.Name.ToLower().Equals(stat.name.ToLower()));
            var statValue = (int)property.GetValue(GameStateStorage.StarterStats);
            property.SetValue(GameStateStorage.StarterStats, statValue + 1);
            NewStatSum += 1;

            UpdateDisplayStatValue(stat, property);
        }

        private void UpdateDisplayStatValue(Object stat, PropertyInfo property)
        {
            ((Text) display.GetType().GetFields().ToList()
                    .Find(p => p.Name.ToLower().Equals(stat.name.ToLower())).GetValue(display))
                .text = property.GetValue(GameStateStorage.StarterStats).ToString();
        }


        public void OnMinusClick(Text stat)
        {
            var property = GameStateStorage.StarterStats.GetType().GetProperties().ToList()
                .Find(p => p.Name.ToLower().Equals(stat.name.ToLower()));
            var statValue = (int)property.GetValue(GameStateStorage.StarterStats);
            if (statValue - 1 > 0)
            {
                property.SetValue(GameStateStorage.StarterStats, statValue - 1);
                NewStatSum -= 1;
            }
            UpdateDisplayStatValue(stat, property);
        }
        
    }
}