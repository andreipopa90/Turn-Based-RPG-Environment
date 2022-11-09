using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using JsonParser;
using UnityEngine;
using System.Linq;
using GenerativeGrammar.Grammar;
using LogFiles;
using UnityEditor;

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
        public List<BaseStat> EnemiesBase { get; set; }
        public List<Nature> EnemiesNature { get; set; }
        public bool LostCurrentLevel { get; set; }

        public Generator Generator;

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
            LostCurrentLevel = false;
            ChooseEnemies();
            GenerateEnemies();
        }

        private void GenerateEnemies()
        {
            var levelLog = Log.GetInstance();
            levelLog.PlayerTypes.Add("Bug");
            // levelLog.PlayerTypes.Add("Dark");
            // levelLog.PlayerTypes.Add("Dragon");
            levelLog.PlayerDefense = "Special";
            levelLog.PlayerAttack = "Special";
            Generator.StartGeneration(levelLog);
        }

        public void ChooseEnemies()
        {
            EnemiesBase = new List<BaseStat>();
            EnemiesNature = new List<Nature>();
            for (var i = 0; i < 3; i++)
            {
                var randomNumber = new System.Random().Next(0, EnemyBaseStats.Count);
                var enemyBase = EnemyBaseStats[randomNumber];
                EnemiesBase.Add(enemyBase);
                randomNumber = new System.Random().Next(0, Natures.Count);
                var enemyNature = Natures[randomNumber];
                EnemiesNature.Add(enemyNature);
                // var enemyUnit = new Unit();
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }


    }
}
