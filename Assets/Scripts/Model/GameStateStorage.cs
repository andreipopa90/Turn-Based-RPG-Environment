using System.Collections.Generic;
using System.Linq;
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
        public string StarterPokemon { get; set; }
        public List<StartMoves> StartMoves { get; set; }
        
        public BaseStat StarterStats { get; set; }

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
            StarterPokemon = string.Empty;
            StartMoves = reader.ReadStartMovesJson();
            StarterStats = new BaseStat();
        }

    

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }


    }
}
