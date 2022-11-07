using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogFiles;
using Model;
using Model.Observer;
using Newtonsoft.Json.Linq;
using UI;
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
        [SerializeField]
        private NPCHealthStatus charactersStatus;
        [SerializeField]
        private BattleSetUp battleSetUp;

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
            battleSetUp.GameState = _gameState;
            battleSetUp.Manager = _manager;
            battleSetUp.LevelLog = _levelLog;
            battleSetUp.SetUpCharacters(player, enemy, charactersStatus, _enemies);
            battleSetUp.SetUpHUD(mainHUD);
            GatherCharactersInScene();
            BeginBattle();
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
            playerUnit.Heal(playerUnit.MaxHealth / 4);
            _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
            mainHUD.AbilityPanel.SetActive(false);
            mainHUD.EnemyPanel.SetActive(false);
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
            mainHUD.AbilityPanel.SetActive(false);
            mainHUD.EnemyPanel.SetActive(false);
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
            _actionQueue = _actionQueue.OrderByDescending(x => x.SourceUnit.Spe).ToList();
        
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
            if (action.SourceUnit.UnitName.Equals("Player"))
            {
                _levelLog.PlayerMovesUsed.Add(action.Move.Name);
            }

            var target = GetTarget(action);

            if (!action.SourceUnit || !target) return;
            HandleAction(action, target);

            charactersStatus.UpdateHealthBar(target);
        }

        private void HandleStatus(Move move, Unit targetUnit, bool buff = true)
        {
            
            if (!targetUnit) return;
            var boosts = ((JObject)move.Secondary["boosts"]).ToObject<Dictionary<string, int>>();
            print(move.Name + " self-buff: " + buff);
            foreach (var key in boosts.Keys)
            {
                if (key.Equals("accuracy") || key.Equals("evasion")) continue;
                var value = boosts[key];
                var stat = (int) targetUnit.GetType()
                    .GetProperties().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .GetValue(targetUnit);
                var newStatValue = value > 0 ? stat * (value + 2) / 2 : stat * 2 / (-1 * value + 2);
                targetUnit.GetType().GetProperties().ToList().Find(p => p.Name.ToLower().Equals(key))
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
                    HandleDebuff(action, target);
                    var damageTaken = HandleDamageTaken(action, target);
                    HandleDrain(action, damageTaken);
                    HandleStatusAilment(action, target);
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
            if (!target.UnitName.Equals("Player")) return 1.0;
            if (domainSource is null) return 1.0;
            if (action.SourceUnit.UnitName.Equals("Player") &&
                target.DetermineMoveEffectiveness(action.Move.MoveType) > 1)
            {
                return 1.0 / target.DetermineMoveEffectiveness(action.Move.MoveType);
            }
            return domainSource.Types.Contains(action.Move.MoveType) ? 1.5 : 1.0;
        }

        private static void HandleDebuff(Action action, Unit target)
        {
            if (!action.Move.Secondary.ContainsKey("debuff")) return;
            var debuff = ((JObject) action.Move.Secondary["debuff"]).ToObject<Dictionary<string, dynamic>>();
            var randomNumber = new System.Random().Next(1, 100);
            if (debuff["chance"] < randomNumber) return;

            var boosts = ((JObject) debuff["boosts"]).ToObject<Dictionary<string, int>>();

            foreach (var key in boosts.Keys)
            {
                var value = boosts[key];
                if (key.Equals("accuracy") || key.Equals("evasion")) continue;
                var stat = (int) target.GetType()
                    .GetProperties().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .GetValue(target);
                var newStatValue = stat * 2 / (-1 * value + 2);
                target.GetType().GetProperties().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .SetValue(action.TargetUnit, newStatValue);
            }
        }

        private static void HandleSelfBuff(Action action)
        {
            if (!action.Move.Secondary.ContainsKey("self-buff")) return;
            var selfBuff = ((JObject) action.Move.Secondary["self-buff"]).ToObject<SelfBuff>();
            var randomNumber = new System.Random().Next(1, 100);
            if (selfBuff.Chance < randomNumber) return;
        
            foreach (var key in selfBuff.Self["boosts"].Keys)
            {
                var value = selfBuff.Self["boosts"][key];
                if (key.Equals("accuracy") || key.Equals("evasion")) continue;
                var stat = (int) action.SourceUnit.GetType()
                    .GetProperties().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .GetValue(action.SourceUnit);
                var newStatValue = stat * (value + 2) / 2;
                action.SourceUnit.GetType().GetProperties().ToList().Find(p => p.Name.ToLower().Equals(key))
                    .SetValue(action.SourceUnit, newStatValue);
            }
        }

        private void HandleStatusAilment(Action action, Unit target)
        {
            if (!action.Move.Secondary.ContainsKey("status")) return;
            
            var random = new System.Random().Next(0, 100);
            var statusEffect = ((JObject) action.Move.Secondary["status"]).ToObject<Dictionary<string, string>>();
            if (random <= int.Parse(statusEffect["chance"]) && !target.Ailments.Contains("ReverseAilment"))
            {
                target.Ailments.Add(statusEffect["status"]);
                if (statusEffect["status"].Equals("par"))
                    target.Spe /= 2;
                charactersStatus.GetPanels()[target].
                    GetComponent<AilmentIndicator>().ShowAilment(statusEffect["status"]);
            }
            else if (random <= int.Parse(statusEffect["chance"]))
            {
                action.SourceUnit.Ailments.Add(statusEffect["status"]);
                if (statusEffect["status"].Equals("par"))
                    action.SourceUnit.Spe /= 2;
                charactersStatus.GetPanels()[action.SourceUnit].
                    GetComponent<AilmentIndicator>().ShowAilment(statusEffect["status"]);
            }
            
        }

        private void HandleDrain(Action action, int damageTaken)
        {
            if (!action.Move.Secondary.ContainsKey("drain")) return;
            var healValue = action.Move.Secondary["drain"] == 1 ? damageTaken / 2 : damageTaken * 3 / 4;
            action.SourceUnit.Heal(healValue);

            if (!action.SourceUnit.Affixes.Contains("Medic")) return;
            foreach (var character in 
                     _sceneCharacters.Where(character => !character.UnitName.Equals("Player") && 
                                                         !character.UnitName.Equals(action.SourceUnit.UnitName)))
            {
                character.Heal(healValue);
            }

            if (!action.SourceUnit.Affixes.Contains("Lifesteal")) return;
            action.SourceUnit.Heal(damageTaken / 2);
            if (!action.SourceUnit.Affixes.Contains("Medic")) return;
            foreach (var character in 
                     _sceneCharacters.Where(character => !character.UnitName.Equals("Player") && 
                                                         !character.UnitName.Equals(action.SourceUnit.UnitName)))
            {
                character.Heal(damageTaken / 2);
            }

        }

        private static Unit GetTarget(Action action)
        {
            var target = action.TargetUnit;
            if (action.TargetUnit || !action.SourceUnit.UnitName.Equals("Player")) return target;
        
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
            if (_sceneCharacters.Count == 1 && _sceneCharacters[0].UnitName.Equals("Player"))
            {
                _currentState = BattleState.Win;
                _gameState.CurrentLevel += 1;
                _gameState.LostCurrentLevel = false;
                _manager.Reset();
                SceneManager.LoadScene("TransitionScene");
            }
            else if (!GameObject.FindGameObjectWithTag("Player"))
            {
                _currentState = BattleState.Lost;
                _gameState.LostCurrentLevel = true;
                _manager.Reset();
                SceneManager.LoadScene("TransitionScene");
            }
            else
            {
                HandleDamageOverTime();
                BeginBattle();
            }
        }

        private void HandleDamageOverTime()
        {
            foreach (var character in _sceneCharacters.Where(character => character))
            {
                if (character.Ailments.Contains("brn"))
                {
                    character.TakeDamage(character.CurrentHealth / 16);
                }

                if (character.Ailments.Contains("psn"))
                {
                    character.TakeDamage(character.CurrentHealth / 8);
                }
            }
        }
    }
}
