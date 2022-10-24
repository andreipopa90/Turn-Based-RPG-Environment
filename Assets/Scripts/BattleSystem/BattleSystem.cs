using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogFiles;
using Model;
using Model.Observer;
using Newtonsoft.Json.Linq;
using UI.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace BattleSystem
{
    public class BattleSystem : MonoBehaviour
    {

        private struct Action
        {
            public Move Move;
            public Unit SourceUnit;
            public Unit TargetUnit;
        }

        private enum BattleState
        {
            Start, Battle, ActionResolve, Win, Lost
        }

        [FormerlySerializedAs("Player")] public GameObject player;
        [FormerlySerializedAs("Enemy")] public GameObject enemy;
        [FormerlySerializedAs("MainHUD")] public BattleHUD mainHUD;
        [FormerlySerializedAs("EnemyStatus")] public NPCHealthStatus enemyStatus;
        public NPCHealthStatus playerStatus;

        private int _currentTurn;
        private List<Unit> _sceneCharacters;
        private List<Unit> _enemies;
        private List<Action> _actionQueue;
        private BattleState _currentState;
        private Move _selectedMove;
        private GameStateStorage _gameState;
        private Log _levelLog;
        private EventManager _manager;

        // Start is called before the first frame update
        private void Start()
        {
            _actionQueue = new List<Action>();
            _currentState = BattleState.Start;
            _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            _levelLog = Log.GetInstance();
            _levelLog.CurrentLevel = _gameState.CurrentLevel;
            _sceneCharacters = new List<Unit>();
            _manager = new EventManager();
            _enemies = new List<Unit>();
            SetUpCharacters();
            SetUpHUD();
            GatherCharactersInScene();
            BeginBattle();
        }

        private void SetUpHUD()
        {
            mainHUD.AddAbilitySelect();
            var enemies = GameObject.FindGameObjectsWithTag("Enemy")
                .Select(go => go.GetComponent<Unit>()).ToList();
            mainHUD.AddEnemySelect(enemies);
        }

        private GameObject InstantiateCharacter(string characterName, Vector3 position, GameObject prefab, 
            bool isPlayer = false, string keyName = "")
        {
            var character = Instantiate(prefab, position, Quaternion.identity);
            character.name = characterName;
            character.GetComponent<Unit>().unitName = characterName;
            var characterUnit = character.GetComponent<Unit>();
            characterUnit.Moves = new List<Move>();
            characterUnit.Affixes = new List<string>();
            characterUnit.Ailments = new List<string>();
            if (!isPlayer)
            {
                var learnSet = 
                    _gameState.StartMoves.Find(sm => sm.Name.Equals(keyName)) ?? 
                    _gameState.StartMoves.Find(sm => keyName.Contains(sm.Name));
                character.GetComponent<Unit>().AddMoves(_gameState.AllMoves, learnSet.LearnSet);
                return character;
            }

            characterUnit.level = _gameState.CurrentLevel + 39;
            characterUnit.types = _gameState.StarterStats.Types;
            characterUnit.Manager = _manager;
            characterUnit.SetStats(_gameState.StarterStats);
            // Add to logs.
            AddPlayerStatsToLogs(characterUnit);

            return character;
        }

        private void AddPlayerStatsToLogs(Unit characterUnit)
        {
            _levelLog.PlayerDefense = characterUnit.def > characterUnit.spd ? "Physical" : "Special";
            _levelLog.PlayerAttack = characterUnit.atk > characterUnit.spa ? "Physical" : "Special";
            _levelLog.PlayerStats["HP"] = characterUnit.maxHealth;
            _levelLog.PlayerStats["ATK"] = characterUnit.atk;
            _levelLog.PlayerStats["DEF"] = characterUnit.def;
            _levelLog.PlayerStats["SPA"] = characterUnit.spa;
            _levelLog.PlayerStats["SPD"] = characterUnit.spd;
            _levelLog.PlayerStats["SPE"] = characterUnit.spe;
        }

        private GameObject SetUpCharacterStats(GameObject character, BaseStat characterBase, Nature characterNature)
        {
            var characterUnit = character.GetComponent<Unit>();
            characterUnit.Manager = _manager;
            characterUnit.level = _gameState.CurrentLevel;
            characterUnit.types = characterBase.Types;
            characterUnit.unitNature = characterNature;
            characterUnit.SetStats(characterBase);
            return character;
        }

        private void SetUpCharacters()
        {
            var playerInstance = 
                InstantiateCharacter("Player", new Vector3(0, 0, 0), player, true);

            _manager.AddListener(playerInstance.GetComponent<Unit>());
            
            for (var i = 0; i < 3; i++)
            {
                var randomNumber = new System.Random().Next(0, _gameState.EnemyBaseStats.Count);
                var enemyBase = _gameState.EnemyBaseStats[randomNumber];
                AddEnemyBaseStatsToLogs(enemyBase);
                
                randomNumber = new System.Random().Next(0, _gameState.Natures.Count);
                var enemyNature = _gameState.Natures[randomNumber];
                var enemyInstance = InstantiateCharacter(enemyBase.Name + " " + (i + 1), 
                    new Vector3(-7 + 7 * i, 0, 10), enemy, keyName: enemyBase.KeyName);
                enemyInstance = SetUpCharacterStats(enemyInstance, enemyBase, enemyNature);
                _enemies.Add(enemyInstance.GetComponent<Unit>());
                _manager.AddListener(enemyInstance.GetComponent<Unit>());
            }
            enemyStatus.SetUpEnemyStatusPanels(_enemies);
            playerStatus.SetUpPlayerStatusPanels(playerInstance.GetComponent<Unit>());
            
        }

        private void AddEnemyBaseStatsToLogs(BaseStat enemyBase)
        {
            _levelLog.EnemyStats["HP"].Add(enemyBase.Hp);
            _levelLog.EnemyStats["ATK"].Add(enemyBase.Atk);
            _levelLog.EnemyStats["DEF"].Add(enemyBase.Def);
            _levelLog.EnemyStats["SPA"].Add(enemyBase.Spa);
            _levelLog.EnemyStats["SPD"].Add(enemyBase.Spd);
            _levelLog.EnemyStats["SPE"].Add(enemyBase.Spe);
        }

        private void GatherCharactersInScene()
        {
            _sceneCharacters = new List<Unit>();
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
            {
                _sceneCharacters.Add(playerObject.GetComponent<Unit>());
            }
            foreach (var enemies in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                if (enemies)
                {
                    _sceneCharacters.Add(enemies.GetComponent<Unit>());
                }
            }
        
        }

        private void BeginBattle()
        {
            _currentState = BattleState.Battle;
            PlayerTurn();
        }

        private void PlayerTurn()
        {
            mainHUD.AttackButton.gameObject.SetActive(true);
            mainHUD.HealButton.gameObject.SetActive(true);
            mainHUD.CureButton.gameObject.SetActive(true);
        }

        private void EnemyTurn()
        {
            
            while (true)
            {
                var currentEnemy = _sceneCharacters[_currentTurn];
                Action action;
                action.Move = currentEnemy.Moves[new System.Random().Next(0, currentEnemy.Moves.Count)];
                action.SourceUnit = currentEnemy;
                action.TargetUnit = GameObject.Find("Player").GetComponent<Unit>();
                _actionQueue.Add(action);

                _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
                if (_currentTurn == 0)
                {
                    StartCoroutine(ResolveAttacks());
                }
                else
                {
                    continue;
                }

                break;
            }
        }

        public void OnAttackButtonPress()
        {
            if (GameObject.FindGameObjectWithTag("Player") == null || 
                (_currentState.Equals(BattleState.Battle) && !_sceneCharacters[_currentTurn].name.Contains("Player")))
                return;

            if (mainHUD.EnemyPanel.activeSelf)
                mainHUD.EnemyPanel.SetActive(false);
            mainHUD.AbilityPanel.SetActive(!mainHUD.AbilityPanel.activeSelf);
        }

        public void OnHealButtonPress()
        {
            if (GameObject.FindGameObjectWithTag("Player") == null || 
                (_currentState.Equals(BattleState.Battle) && !_sceneCharacters[_currentTurn].name.Contains("Player")))
                return;
            var playerUnit = _sceneCharacters[0].GetComponent<Unit>();
            playerUnit.Heal(playerUnit.maxHealth / 4);
            _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
            EnemyTurn();
        }

        public void OnCureButton()
        {
            if (GameObject.FindGameObjectWithTag("Player") == null || 
                (_currentState.Equals(BattleState.Battle) && !_sceneCharacters[_currentTurn].name.Contains("Player")))
                return;
            var playerUnit = _sceneCharacters[0].GetComponent<Unit>();
            playerUnit.Cure();
            _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
            EnemyTurn();
        }

        public void OnAbilityButtonPress()
        {
            var moveName = EventSystem.current.currentSelectedGameObject.name;
            _selectedMove = _gameState.SelectedMoves.Find(x => x.Name.Equals(moveName));

            mainHUD.AbilityPanel.SetActive(!mainHUD.AbilityPanel.activeSelf);
            mainHUD.EnemyPanel.SetActive(true);

        }

        public void OnEnemySelect()
        {
            Action action;
            action.Move = _selectedMove;
            action.SourceUnit = _sceneCharacters[0].GetComponent<Unit>();
            action.TargetUnit = GameObject.Find(EventSystem.current.currentSelectedGameObject.name).GetComponent<Unit>();
            _actionQueue.Add(action);

            mainHUD.EnemyPanel.SetActive(false);
            _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;

            EnemyTurn();
        }

        private IEnumerator ResolveAttacks()
        {
            mainHUD.AttackButton.gameObject.SetActive(false);
            mainHUD.HealButton.gameObject.SetActive(false);
            mainHUD.CureButton.gameObject.SetActive(false);
            _currentState = BattleState.ActionResolve;
            _actionQueue = _actionQueue.OrderByDescending(x => x.SourceUnit.spe).ToList();
        
            foreach (var action in _actionQueue)
            {
                HandleAction(action);
                yield return new WaitForSeconds(0.75f);
            }
            _actionQueue.Clear();
            CheckCurrentBattleState();
        }
    
        private void HandleAction(Action action)
        {
            if (action.SourceUnit.unitName.Equals("Player"))
            {
                _levelLog.PlayerMovesUsed.Add(action.Move.Name);
            }

            var target = GetTarget(action);

            if (!action.SourceUnit || !target) return;
            HandleAction(action, target);

            if (!target.unitName.Equals("Player"))
            {
                enemyStatus.UpdateHealthBar(target);
            }
            else
            {
                playerStatus.UpdateHealthBar(target);
            }
        }

        private void HandleStatus(Move move, Unit targetUnit, bool buff = true)
        {
            if (!targetUnit) return;
            var boosts = ((JObject)move.Secondary["boosts"]).ToObject<Dictionary<string, int>>();
            foreach (var key in boosts.Keys)
            {
                var value = boosts[key];
                var stat = (int)targetUnit.GetType()
                    .GetFields().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .GetValue(targetUnit);
                var newStatValue = buff ? stat * (value + 2) / 2 : stat * 2 / (-1 * value + 2);
                targetUnit.GetType().GetFields().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .SetValue(targetUnit, newStatValue);
            }
        }

        private void HandleAction(Action action, Unit target)
        {
            switch (action.Move.Category)
            {
                case "Status" when action.Move.Target.Equals("self") && action.Move.Secondary.ContainsKey("boosts"):
                {
                    HandleStatus(action.Move, action.SourceUnit);
                    break;
                }
                case "Status" when !action.Move.Target.Equals("self") && action.Move.Secondary.ContainsKey("boosts"):
                {
                    HandleStatus(action.Move, action.TargetUnit, false);
                    break;
                }
                default:
                {
                    HandleSelfBuff(action);
                    HandleDebuff(action);
                    var damageTaken = HandleDamageTaken(action, target);
                    HandleDrain(action, damageTaken);
                    HandleStatusAilment(action);
                    break;
                }
            }
        }

        private int HandleDamageTaken(Action action, Unit target)
        {
            var domainEffect = HandleDomain(action, target);
            var burnEffect = 
                action.SourceUnit.Ailments.Contains("brn") && action.Move.Category.Equals("Physical")
                ? 0.5
                : 1.0;
            if (!target.Ailments.Contains("HealthLink"))
            {
                return target.TakeDamage(action.Move, action.SourceUnit, multiplier: domainEffect * burnEffect);
            }
            
            var characters = _sceneCharacters.
                Where(sc => sc.Ailments.Contains("HealthLink")).ToList();
            var damageTaken = 0;
            foreach (var character in characters)
            {
                damageTaken = character.TakeDamage(action.Move, action.SourceUnit, 
                    multiplier: Math.Round(1.0 / characters.Count, 2) * domainEffect * burnEffect);
            }

            return damageTaken;
        }

        private double HandleDomain(Action action, Unit target)
        {
            var domainSource = _enemies.Find(e => e.Affixes.Contains("Domain"));
            if (!target.unitName.Equals("Player")) return 1.0;
            if (domainSource is null) return 1.0;
            if (action.SourceUnit.unitName.Equals("Player") &&
                target.DetermineMoveEffectiveness(action.Move.MoveType) > 1)
            {
                return 1.0 / target.DetermineMoveEffectiveness(action.Move.MoveType);
            }
            return domainSource.types.Contains(action.Move.MoveType) ? 1.5 : 1.0;
        }

        private void HandleDebuff(Action action)
        {
            if (!action.Move.Secondary.ContainsKey("debuff")) return;
            var debuff = ((JObject) action.Move.Secondary["debuff"]).ToObject<Dictionary<string, dynamic>>();
            var randomNumber = new System.Random().Next(1, 100);
            if (debuff["chance"] < randomNumber) return;

            var boosts = ((JObject) debuff["boosts"]).ToObject<Dictionary<string, int>>();
            
            foreach (var key in boosts.Keys)
            {
                var value = boosts[key];
                var stat = (int) action.TargetUnit.GetType()
                    .GetFields().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .GetValue(action.TargetUnit);
                var newStatValue = stat * 2 / (-1 * value + 2);
                action.TargetUnit.GetType().GetFields().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .SetValue(action.TargetUnit, newStatValue);
            }
        }

        private void HandleSelfBuff(Action action)
        {
            if (!action.Move.Secondary.ContainsKey("self-buff")) return;
            var selfBuff = ((JObject) action.Move.Secondary["self-buff"]).ToObject<SelfBuff>();
            var randomNumber = new System.Random().Next(1, 100);
            if (selfBuff.Chance < randomNumber) return;
        
            foreach (var key in selfBuff.Self["boosts"].Keys)
            {
                var value = selfBuff.Self["boosts"][key];
                var stat = (int) action.SourceUnit.GetType()
                    .GetFields().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .GetValue(action.SourceUnit);
                var newStatValue = stat * (value + 2) / 2;
                action.SourceUnit.GetType().GetFields().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .SetValue(action.SourceUnit, newStatValue);
            }
        }

        private void HandleStatusAilment(Action action)
        {
            if (!action.Move.Secondary.ContainsKey("status")) return;
        
            var random = new System.Random().Next(0, 100);
            var statusEffect = ((JObject) action.Move.Secondary["status"]).ToObject<Dictionary<string, string>>();
            if (random <= int.Parse(statusEffect["chance"]) && !action.TargetUnit.Ailments.Contains("ReverseAilment"))
                action.TargetUnit.Ailments.Add(statusEffect["status"]);
            else
            {
                action.SourceUnit.Ailments.Add(statusEffect["status"]);
            }
            if (statusEffect["status"].Equals("par"))
                action.TargetUnit.spe /= 2;
        }

        private void HandleDrain(Action action, int damageTaken)
        {
            if (!action.Move.Secondary.ContainsKey("drain")) return;
            var healValue = action.Move.Secondary["drain"] == 1 ? damageTaken / 2 : damageTaken * 3 / 4;
            action.SourceUnit.Heal(healValue);

            if (!action.SourceUnit.Affixes.Contains("Medic")) return;
            foreach (var character in 
                     _sceneCharacters.Where(character => !character.unitName.Equals("Player") && 
                                                         !character.unitName.Equals(action.SourceUnit.unitName)))
            {
                character.Heal(healValue);
            }

            if (!action.SourceUnit.Affixes.Contains("Lifesteal")) return;
            action.SourceUnit.Heal(damageTaken / 2);
            if (!action.SourceUnit.Affixes.Contains("Medic")) return;
            foreach (var character in 
                     _sceneCharacters.Where(character => !character.unitName.Equals("Player") && 
                                                         !character.unitName.Equals(action.SourceUnit.unitName)))
            {
                character.Heal(damageTaken / 2);
            }

        }

        private Unit GetTarget(Action action)
        {
            var target = action.TargetUnit;
            if (action.TargetUnit || !action.SourceUnit.unitName.Equals("Player")) return target;
        
            var enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
            var randomEnemy = new System.Random().Next(0, enemiesArray.Length);
            if (enemiesArray.Length > 0)
            {
                target = enemiesArray[randomEnemy].GetComponent<Unit>();
            }

            return target;
        }

        private void CheckCurrentBattleState()
        {
            GatherCharactersInScene();
            if (_sceneCharacters.Count == 1 && _sceneCharacters[0].unitName.Equals("Player"))
            {
                _currentState = BattleState.Win;
                _gameState.CurrentLevel += 1;
                _manager.Reset();
                SceneManager.LoadScene("TransitionScene");
            }
            else if (!GameObject.FindGameObjectWithTag("Player"))
            {
                _currentState = BattleState.Lost;
                _manager.Reset();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                HandleDamageOverTime();
                BeginBattle();
            }
        }

        private void HandleDamageOverTime()
        {
            foreach (var character in _sceneCharacters)
            {
                if (character.Ailments.Contains("brn"))
                {
                    character.TakeDamage(character.currentHealth / 16);
                }

                if (character.Ailments.Contains("psn"))
                {
                    character.TakeDamage(character.currentHealth / 8);
                }
            }
        }
    }
}
