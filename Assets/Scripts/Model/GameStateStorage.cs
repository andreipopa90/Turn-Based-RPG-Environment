using System.Collections.Generic;
using System.Linq;
using JsonParser;
using UnityEngine;
using GenerativeGrammar.Grammar;
using LogFiles;

namespace Model
{
    public class GameStateStorage : MonoBehaviour
    {
        public List<Move> AllMoves { get; set; }
        public List<Move> SelectedMoves { get; set; }
        public int CurrentLevel { get; set; }
        public List<BaseStat> BaseStats { get; set; }
        public List<Type> TypeChart { get; set; }
        public List<Nature> Natures { get; set; }
        public string StarterPokemon { get; set; }
        public List<StartMoves> StartMoves { get; set; }
        
        public BaseStat StarterStats { get; set; }
        public List<BaseStat> EnemiesBase { get; set; }
        public List<Nature> EnemiesNature { get; set; }
        public bool LostCurrentLevel { get; set; }
        public List<List<string>> EnemiesAffixes { get; set; }
        public List<List<Move>> EnemiesMoves { get; set; }
        public List<Dictionary<string, int>> EnemiesEvs { get; set; }
        public List<List<Type>> EnemiesTypes { get; set; }

        private Generator _generator;
        public Log LevelLog { get; set; }
        public bool Dynamic { get; set; }

        // Start is called before the first frame update
        private void Start()
        {
            Dynamic = true;
            LevelLog = Log.GetInstance();
            SelectedMoves = new List<Move>();
            CurrentLevel = 1;
            JSONReader reader = new();
            BaseStats = reader.ReadBaseStatsJson();
            TypeChart = reader.ReadTypeChartJson();
            AllMoves = reader.ReadMovesJson();
            Natures = reader.ReadNaturesJson();
            StarterPokemon = string.Empty;
            StartMoves = reader.ReadStartMovesJson();
            StarterStats = new BaseStat();
            LostCurrentLevel = false;
            _generator = new Generator();
            CreateLevel();
        }

        public void CreateLevel()
        {
            switch (Dynamic)
            {
                case true when CurrentLevel > 1:
                {
                    GenerateEnemies();
                    break;
                }
                case false:
                case true when CurrentLevel == 1:
                    ChooseEnemies();
                    break;
            }
        }

        public void GenerateEnemies()
        {
            _generator.StartGeneration(LevelLog);
            NpcsToUnits();
        }

        private void NpcsToUnits()
        {
            EnemiesBase = new List<BaseStat>();
            EnemiesNature = new List<Nature>();
            EnemiesMoves = new List<List<Move>>();
            EnemiesAffixes = new List<List<string>>();
            EnemiesEvs = new List<Dictionary<string, int>>();
            EnemiesTypes = new List<List<Type>>();
            for (var i = 0; i < _generator.Npcs.Count; i++)
            { 
                var npc = _generator.Npcs[i].ValuesOfNodes;
                print(_generator.Npcs[i].ToString());
                EnemiesBase.Add(npc["BASE"][0]);
                EnemiesNature.Add(npc["NATURE"][0]);
                var affixes = npc["AFFIX"].Cast<string>().ToList();
                EnemiesAffixes.Add(affixes);
                EnemiesMoves.Add(npc["MOVE"].Cast<Move>().ToList());
                var evs = new Dictionary<string, int>()
                {
                    {"hp", npc["HPEV"][0]},
                    {"atk", npc["ATKEV"][0]},
                    {"def", npc["DEFEV"][0]},
                    {"spa", npc["SPAEV"][0]},
                    {"spd", npc["SPDEV"][0]},
                    {"spe", npc["SPEEV"][0]}
                };
                EnemiesEvs.Add(evs);
                EnemiesTypes.Add(npc["TYPE"].Cast<Type>().ToList());
            }
        }

        public void ChooseEnemies()
        {
            EnemiesBase = new List<BaseStat>();
            EnemiesNature = new List<Nature>();
            EnemiesMoves = new List<List<Move>>();
            EnemiesTypes = new List<List<Type>>();
            EnemiesEvs = new List<Dictionary<string, int>>();
            EnemiesAffixes = new List<List<string>>();
            for (var i = 0; i < 3; i++)
            {
                var randomNumber = new System.Random().
                    Next(BaseStats.Count * i / 10, BaseStats.Count * (i + 1) / 10);
                var enemyBase = BaseStats[randomNumber];
                EnemiesBase.Add(enemyBase);
                randomNumber = new System.Random().Next(0, Natures.Count);
                var enemyNature = Natures[randomNumber];
                EnemiesNature.Add(enemyNature);
                var keyName = enemyBase.KeyName;
                var learnSet = 
                    StartMoves.Find(sm => sm.Name.Equals(keyName)) ?? 
                    StartMoves.Find(sm => keyName.Contains(sm.Name));
                var newMoves = AllMoves.Where(m => learnSet.LearnSet.Contains(m.KeyName)).ToList();
                var randomSeed = new System.Random();
                newMoves = newMoves.OrderBy(a => randomSeed.Next()).Take(4).ToList();
                var ev = new Dictionary<string, int>
                {
                    {"hp", 0},
                    {"atk", 0},
                    {"def", 0},
                    {"spa", 0},
                    {"spd", 0},
                    {"spe", 0}
                };
                EnemiesEvs.Add(ev);
                EnemiesMoves.Add(newMoves);
                EnemiesTypes.Add(TypeChart.FindAll(t => enemyBase.Types.Contains(t.Name)));
                EnemiesAffixes.Add(new List<string> {string.Empty});
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }


    }
}
