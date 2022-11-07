using System.Collections.Generic;
using JsonParser;
using UnityEngine;
using System.Linq;

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
        public List<Unit> Enemies { get; set; }
        public List<BaseStat> EnemiesBase { get; set; }

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
            // print("I started again!");
            // ChooseEnemies();
        }

        private void ChooseEnemies()
        {
            for (var i = 0; i < 3; i++)
            {
                var randomNumber = new System.Random().Next(0, EnemyBaseStats.Count);
                var enemyBase = EnemyBaseStats[randomNumber];
                EnemiesBase.Add(enemyBase);
                randomNumber = new System.Random().Next(0, Natures.Count);
                var enemyNature = Natures[randomNumber];
                // var enemyUnit = new Unit();
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }


    }
}
