using System;
using System.Collections.Generic;

namespace LogFiles
{
    [Serializable]
    public class Log
    {
        private static Log _instance;
        public int CurrentLevel { get; set; }
        public Dictionary<string, List<int>> EnemyStats { get; set; }
        public Dictionary<string, int> PlayerStats { get; set; }
        public bool HasAilments { get; set; }
        public List<string> PlayerTypes { get; set; }
        public string PlayerDefense { get; set; }
        public string PlayerAttack { get; set; }
        public List<string> PlayerMovesUsed { get; set; }
        public static Log GetInstance()
        {
            if (_instance == null) _instance = new Log();
            return _instance;
        }

        private Log()
        {
            EnemyStats = new Dictionary<string, List<int>>();
            EnemyStats.Add("HP", new List<int>());
            EnemyStats.Add("ATK", new List<int>());
            EnemyStats.Add("DEF", new List<int>());
            EnemyStats.Add("SPA", new List<int>());
            EnemyStats.Add("SPD", new List<int>());
            EnemyStats.Add("SPE", new List<int>());
            PlayerStats = new Dictionary<string, int>();
            HasAilments = false;
            PlayerTypes = new List<string>();
            PlayerDefense = string.Empty;
            PlayerAttack = string.Empty;
            PlayerMovesUsed = new List<string>();
        }
    }
}