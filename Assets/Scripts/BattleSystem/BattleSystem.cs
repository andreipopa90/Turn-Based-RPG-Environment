using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using UI;
using UI.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Action = Model.Action;

namespace BattleSystem
{
    public class BattleSystem : MonoBehaviour
    {
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
        [SerializeField]
        private BattleLog battleLog;
        [SerializeField]
        private BattleHandler battleHandler;

        private int _currentTurn;
        private List<Unit> _sceneCharacters;
        private List<Unit> _enemies;
        private List<Action> _actionQueue;
        private BattleState _currentState;
        private Move _selectedMove;
        private GameStateStorage _gameState;
        
        
        // Start is called before the first frame update
        private void Start()
        {
            _actionQueue = new List<Action>();
            _currentState = BattleState.Start;
            _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            _gameState.LevelLog.CurrentLevel = _gameState.CurrentLevel;
            _sceneCharacters = new List<Unit>();
            _enemies = new List<Unit>();
            
            battleSetUp.GameState = _gameState;
            battleSetUp.LevelLog = _gameState.LevelLog;
            battleSetUp.SetUpCharacters(player, enemy, charactersStatus, _enemies);
            battleSetUp.SetUpHUD(mainHUD);
            GatherCharactersInScene();
            
            battleHandler.GameState = _gameState;
            battleHandler.SceneCharacters = _sceneCharacters;
            battleHandler.Enemies = _enemies;
            battleHandler.CharactersStatus = charactersStatus;
            battleHandler.BattleLog = battleLog;
            
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
            
            while (_currentTurn != 0)
            {
                var currentEnemy = _sceneCharacters[_currentTurn];
                Action action;
                action.SourceUnit = currentEnemy;
                action.TargetUnit = GameObject.Find("Player").GetComponent<Unit>();
                action.Move = currentEnemy.ChooseMove(action.TargetUnit);
                _actionQueue.Add(action);
                _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
            }
            StartCoroutine(ResolveAttacks());
        }

        public void OnAttackButtonPress()
        {
            if (GameObject.FindGameObjectWithTag("Player") == null || 
                (_currentState.Equals(BattleState.Battle) && 
                 !_sceneCharacters[_currentTurn].name.Contains("Player")))
                return;

            if (mainHUD.EnemyPanel.activeSelf)
                mainHUD.EnemyPanel.SetActive(false);
            mainHUD.AbilityPanel.SetActive(!mainHUD.AbilityPanel.activeSelf);
        }

        public void OnHealButtonPress()
        {
            if (GameObject.FindGameObjectWithTag("Player") == null || 
                (_currentState.Equals(BattleState.Battle) && 
                 !_sceneCharacters[_currentTurn].name.Contains("Player")))
                return;
            var playerUnit = _sceneCharacters[0].GetComponent<Unit>();
            playerUnit.Heal(playerUnit.MaxHealth / 2);
            charactersStatus.UpdateHealthBar(playerUnit);
            _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
            mainHUD.AbilityPanel.SetActive(false);
            mainHUD.EnemyPanel.SetActive(false);
            _gameState.GameStatistics.AddMoveUsed(_gameState.CurrentLevel, "Heal");
            EnemyTurn();
        }

        public void OnCureButton()
        {
            if (GameObject.FindGameObjectWithTag("Player") == null || 
                (_currentState.Equals(BattleState.Battle) && 
                 !_sceneCharacters[_currentTurn].name.Contains("Player")))
                return;
            var playerUnit = _sceneCharacters[0].GetComponent<Unit>();
            playerUnit.Cure();
            _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
            mainHUD.AbilityPanel.SetActive(false);
            mainHUD.EnemyPanel.SetActive(false);
            _gameState.GameStatistics.AddMoveUsed(_gameState.CurrentLevel, "Cure");
            battleHandler.CharactersStatus.GetPanels()[_sceneCharacters[_currentTurn]].
                GetComponent<AilmentIndicator>().HideAilments();
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
            action.TargetUnit = 
                GameObject.Find(EventSystem.current.currentSelectedGameObject.name).GetComponent<Unit>();
            _actionQueue.Add(action);

            mainHUD.EnemyPanel.SetActive(false);
            _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;
            _gameState.GameStatistics.AddMoveUsed(_gameState.CurrentLevel, action.Move.Name);
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
                yield return battleHandler.HandleAction(action);
            }
            _actionQueue.Clear();
            StartCoroutine(CheckCurrentBattleState());
        }

        private IEnumerator CheckCurrentBattleState()
        {
            GatherCharactersInScene();
            if (_sceneCharacters.Count == 1 && _sceneCharacters[0].UnitName.Equals("Player"))
            {
                _currentState = BattleState.Win;
                _gameState.CurrentLevel += 1;
                _gameState.LostCurrentLevel = false;
                _gameState.Manager.Reset();
                SceneManager.LoadScene("TransitionScene");
            }
            else if (!GameObject.FindGameObjectWithTag("Player"))
            {
                yield return battleHandler.PlayerHasDied();
                _currentState = BattleState.Lost;
                _gameState.LostCurrentLevel = true;
                _gameState.Manager.Reset();
                SceneManager.LoadScene("TransitionScene");
            }
            else
            {
                yield return battleHandler.HandleDamageOverTime();
                battleLog.Hide();
                BeginBattle();
            }
        }
    }
}
