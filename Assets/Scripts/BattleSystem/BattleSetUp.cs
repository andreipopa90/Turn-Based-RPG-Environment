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
        
        private GameObject SetUpCharacterStats(GameObject character, BaseStat characterBase, Nature characterNature)
        {
            var characterUnit = character.GetComponent<Unit>();
            characterUnit.Manager = Manager;
            characterUnit.Level = GameState.CurrentLevel;
            characterUnit.SetTypes(characterBase.Types);
            characterUnit.unitNature = characterNature;
            characterUnit.SetStats(characterBase);
            return character;
        }

        public void SetUpCharacters(GameObject player, GameObject enemy, NPCHealthStatus charactersStatus, List<Unit> enemies)
        {
            var playerInstance = 
                InstantiateCharacter("Player", new Vector3(0, 0, -12.5f), player, true);

            Manager.AddListener(playerInstance.GetComponent<Unit>());
            
            for (var i = 0; i < 3; i++)
            {
                var randomNumber = new System.Random().Next(0, GameState.EnemyBaseStats.Count);
                var enemyBase = GameState.EnemyBaseStats[randomNumber];
                AddEnemyBaseStatsToLogs(enemyBase);
                
                randomNumber = new System.Random().Next(0, GameState.Natures.Count);
                var enemyNature = GameState.Natures[randomNumber];
                var enemyInstance = InstantiateCharacter(enemyBase.Name + " " + (i + 1), 
                    new Vector3(-12.5f + 12.5f * i, 0, 12.5f), enemy, keyName: enemyBase.KeyName);
                enemyInstance = SetUpCharacterStats(enemyInstance, enemyBase, enemyNature);
                enemies.Add(enemyInstance.GetComponent<Unit>());
                Manager.AddListener(enemyInstance.GetComponent<Unit>());
            }
            charactersStatus.SetUpEnemyStatusPanels(enemies);
            charactersStatus.SetUpPlayerStatusPanels(playerInstance.GetComponent<Unit>());
            
        }
        
        private GameObject InstantiateCharacter(string characterName, Vector3 position, GameObject prefab, 
            bool isPlayer = false, string keyName = "")
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
                var learnSet = 
                    GameState.StartMoves.Find(sm => sm.Name.Equals(keyName)) ?? 
                    GameState.StartMoves.Find(sm => keyName.Contains(sm.Name));
                character.GetComponent<Unit>().AddMoves(GameState.AllMoves, learnSet.LearnSet);
                return character;
            }

            characterUnit.Level = GameState.CurrentLevel + 10;
            characterUnit.SetTypes(GameState.StarterStats.Types);
            characterUnit.Manager = Manager;
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