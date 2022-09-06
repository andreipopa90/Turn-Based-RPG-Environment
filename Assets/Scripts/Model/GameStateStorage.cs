using System.Collections.Generic;
using JsonParser;
using UnityEngine;

namespace Model
{
    public class GameStateStorage : MonoBehaviour
    {
        public List<Move> AllMoves { get; set; }
        public List<Move> SelectedMoves { get; set; }
        public int CurrentLevel { get; set; }
        public List<BaseStat> EnemyBaseStats { get; set; }
        public List<Type> TypeChart { get; set; }
        public List<Nature> Natures { get; set; }

        // Start is called before the first frame update
        private void Start()
        {
            SelectedMoves = new List<Move>();
            CurrentLevel = 1;
            JSONReader reader = new();
            EnemyBaseStats = reader.ReadBaseStatsJson();
            TypeChart = reader.ReadTypeChartJson();
            AllMoves = reader.ReadMovesJson();
            Natures = reader.ReadNaturesJson();
        }

    

        private void Awake()
        {
            DontDestroyOnLoad(GameObject.Find("GameState"));
        }


    }
}
