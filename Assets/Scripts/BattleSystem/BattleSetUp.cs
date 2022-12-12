using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LogFiles;
using Model;
using Model.Observer;
using UI.Battle;
using UnityEngine;

namespace BattleSystem
{
    public class BattleSetUp : MonoBehaviour
    {
        private static BattleSetUp _instance;
        public GameStateStorage GameState { get; set; }
        public EventManager Manager { get; set; }
        public Log LevelLog { get; set; }

        public void SetUpHUD(BattleHUD battleHUD)
        {
            battleHUD.AddAbilitySelect();
            var enemies = GameObject.FindGameObjectsWithTag("Enemy")
                .Select(go => go.GetComponent<Unit>()).ToList();
            battleHUD.AddEnemySelect(enemies);
        }
        
        private GameObject SetUpCharacterStats(GameObject character, BaseStat characterBase, 
            Nature characterNature, List<string> affixes, List<Move> enemyMoves, List<Type> enemyTypes, 
            Dictionary<string, int> enemyEvs)
        {
            var characterUnit = character.GetComponent<Unit>();
            characterUnit.Manager = Manager;
            characterUnit.Level = GameState.CurrentLevel;
            characterUnit.SetTypes(enemyTypes.Select(t => t.Name).ToList());
            characterUnit.unitNature = characterNature;
            characterUnit.Moves = enemyMoves;
            characterUnit.Ev = enemyEvs;
            characterUnit.Affixes = affixes;
            characterUnit.SetStats(characterBase);
            characterUnit.OriginalStats = characterBase;
            
            return character;
        }

        public void SetUpCharacters(GameObject player, GameObject enemy, NPCHealthStatus charactersStatus, List<Unit> enemies)
        {
            var playerInstance = 
                InstantiateCharacter("Player", new Vector3(0, 0, -12.5f), player, true);

            Manager.AddListener(playerInstance.GetComponent<Unit>());

            for (var i = 0; i < 3; i++)
            {
                AddEnemyBaseStatsToLogs(GameState.EnemiesBase[i]);
                
                var enemyInstance = InstantiateCharacter(GameState.EnemiesBase[i].Name + " " + (i + 1), 
                    new Vector3(-12.5f + 12.5f * i, 0, 12.5f), enemy);
                enemyInstance = SetUpCharacterStats(enemyInstance, GameState.EnemiesBase[i], GameState.EnemiesNature[i],
                    GameState.EnemiesAffixes[i], GameState.EnemiesMoves[i], 
                    GameState.EnemiesTypes[i], GameState.EnemiesEvs[i]);
                enemies.Add(enemyInstance.GetComponent<Unit>());
                Manager.AddListener(enemyInstance.GetComponent<Unit>());
            }
            charactersStatus.SetUpEnemyStatusPanels(enemies);
            charactersStatus.SetUpPlayerStatusPanels(playerInstance.GetComponent<Unit>());
            
        }
        
        private GameObject InstantiateCharacter(string characterName, Vector3 position, GameObject prefab, 
            bool isPlayer = false)
        {
            var character = Instantiate(prefab, position, Quaternion.identity);
            character.name = characterName;
            character.GetComponent<Unit>().UnitName = characterName;
            var characterUnit = character.GetComponent<Unit>();
            characterUnit.Moves = new List<Move>();
            characterUnit.Affixes = new List<string>();
            characterUnit.Ailments = new List<string>();
            if (!isPlayer)
            {
                return character;
            }

            characterUnit.Level = GameState.CurrentLevel + 6;
            characterUnit.SetTypes(GameState.StarterStats.Types);
            characterUnit.Manager = Manager;
            characterUnit.OriginalStats = GameState.StarterStats;
            
            characterUnit.Ev = new Dictionary<string, int>
            {
                {"hp", 0},
                {"atk", 0},
                {"def", 0},
                {"spa", 0},
                {"spd", 0},
                {"spe", 0}
            };
            characterUnit.SetStats(GameState.StarterStats);
            // Add to logs.
            AddPlayerStatsToLogs(characterUnit);

            return character;
        }
        
        private void AddPlayerStatsToLogs(Unit characterUnit)
        {
            LevelLog.PlayerDefense = characterUnit.Def > characterUnit.Spd ? "Physical" : "Special";
            LevelLog.PlayerAttack = characterUnit.Atk > characterUnit.Spa ? "Physical" : "Special";
            LevelLog.PlayerStats["HP"] = characterUnit.MaxHealth;
            LevelLog.PlayerStats["ATK"] = characterUnit.Atk;
            LevelLog.PlayerStats["DEF"] = characterUnit.Def;
            LevelLog.PlayerStats["SPA"] = characterUnit.Spa;
            LevelLog.PlayerStats["SPD"] = characterUnit.Spd;
            LevelLog.PlayerStats["SPE"] = characterUnit.Spe;
            LevelLog.PlayerTypes = characterUnit.Types;
        }
        
        private void AddEnemyBaseStatsToLogs(BaseStat enemyBase)
        {
            LevelLog.EnemyStats["HP"].Add(enemyBase.Hp);
            LevelLog.EnemyStats["ATK"].Add(enemyBase.Atk);
            LevelLog.EnemyStats["DEF"].Add(enemyBase.Def);
            LevelLog.EnemyStats["SPA"].Add(enemyBase.Spa);
            LevelLog.EnemyStats["SPD"].Add(enemyBase.Spd);
            LevelLog.EnemyStats["SPE"].Add(enemyBase.Spe);
        }
        
    }
}