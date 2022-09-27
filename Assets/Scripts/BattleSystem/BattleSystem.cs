using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogFiles;
using Model;
using UI.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
    private List<Action> _actionQueue;
    private BattleState _currentState;
    private Move _selectedMove;
    private GameStateStorage _gameState;
    private Log _levelLog;

    // Start is called before the first frame update
    private void Start()
    {
        _actionQueue = new List<Action>();
        _currentState = BattleState.Start;
        _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
        _levelLog = Log.GetInstance();
        _levelLog.CurrentLevel = _gameState.CurrentLevel;
        _sceneCharacters = new List<Unit>();
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

    private GameObject InstantiateCharacter(string characterName, Vector3 position, GameObject prefab, bool isPlayer = false)
    {
        var character = Instantiate(prefab, position, Quaternion.identity);
        character.name = characterName;
        character.GetComponent<Unit>().unitName = characterName;
        var characterUnit = character.GetComponent<Unit>();
        if (!isPlayer) return character;

        characterUnit.level = _gameState.CurrentLevel + 39;
        characterUnit.types = _gameState.StarterStats.Types;
        characterUnit.SetStats(_gameState.StarterStats);
        // Add to logs.
        _levelLog.PlayerDefense = characterUnit.def > characterUnit.spd ? "Physical" : "Special";
        _levelLog.PlayerAttack = characterUnit.atk > characterUnit.spa ? "Physical" : "Special";
        _levelLog.PlayerStats["HP"] = characterUnit.maxHealth;
        _levelLog.PlayerStats["ATK"] =  characterUnit.atk;
        _levelLog.PlayerStats["DEF"] =  characterUnit.def;
        _levelLog.PlayerStats["SPA"] =  characterUnit.spa;
        _levelLog.PlayerStats["SPD"] =  characterUnit.spd;
        _levelLog.PlayerStats["SPE"] =  characterUnit.spe;

        return character;
    }

    private GameObject SetUpCharacterStats(GameObject character, BaseStat characterBase, Nature characterNature)
    {
        var characterUnit = character.GetComponent<Unit>();
        characterUnit.level = _gameState.CurrentLevel;
        characterUnit.types = characterBase.Types;
        characterUnit.unitNature = characterNature;
        characterUnit.SetStats(characterBase);
        return character;
    }

    private void SetUpCharacters()
    { 
        List<Unit> enemies = new();

        var playerInstance = InstantiateCharacter("Player", new Vector3(0, 0, 0), player, true);

        for (var i = 0; i < 3; i++)
        {
            var randomNumber = new System.Random().Next(0, _gameState.EnemyBaseStats.Count);
            var enemyBase = _gameState.EnemyBaseStats[randomNumber];
            AddEnemyBaseStatsToLogs(enemyBase);
            randomNumber = new System.Random().Next(0, _gameState.Natures.Count);
            var enemyNature = _gameState.Natures[randomNumber];
            var enemyInstance = InstantiateCharacter(enemyBase.Name + " " + (i + 1), 
                    new Vector3(-7 + 7 * i, 0, 10), enemy);
            enemyInstance = SetUpCharacterStats(enemyInstance, enemyBase, enemyNature);
            enemies.Add(enemyInstance.GetComponent<Unit>());
        }
        enemyStatus.SetUpEnemyStatusPanels(enemies);
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
    }

    private void EnemyTurn()
    {
        while (true)
        {
            var currentEnemy = _sceneCharacters[_currentTurn];
            Action action;
            action.Move = _gameState.AllMoves[new System.Random().Next(0, 6)];
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
        if (GameObject.FindGameObjectWithTag("Player") == null || (_currentState.Equals(BattleState.Battle) && !_sceneCharacters[_currentTurn].name.Contains("Player")))
			return;

		if (mainHUD.EnemyPanel.activeSelf)
		    mainHUD.EnemyPanel.SetActive(false);
        mainHUD.AbilityPanel.SetActive(!mainHUD.AbilityPanel.activeSelf);
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

        var target = action.TargetUnit;
        if (!action.TargetUnit && action.SourceUnit.unitName.Equals("Player"))
        {
            var enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
            var randomEnemy = new System.Random().Next(0, enemiesArray.Length);
            if (enemiesArray.Length > 0)
            {
                target = enemiesArray[randomEnemy].GetComponent<Unit>();
            }
        }

        if (!action.SourceUnit || !target) return;
        if (action.Move.Category.Equals("Status") 
            && action.Move.Target.Equals("self") 
            && action.Move.Boosts is not null)
        {
            foreach (var key in action.Move.Boosts.Keys)
            {
                var value = action.Move.Boosts[key];
                var stat = (int) action.SourceUnit.GetType().GetFields().ToList().Find(p => p.Name.ToLower().Equals(key)).GetValue(action.SourceUnit);
                action.SourceUnit.GetType().GetFields().ToList().Find(p => p.Name.ToLower().Equals(key)).SetValue(action.SourceUnit, stat * (value + 2) / 2);
            }
            
        }
        else if (action.Move.Category.Equals("Status") 
                 && !action.Move.Target.Equals("self") 
                 && action.Move.Boosts is not null)
        {
            foreach (var key in action.Move.Boosts.Keys)
            {
                var value = action.Move.Boosts[key];
                var stat = (int) action.TargetUnit.GetType().GetFields().ToList().Find(p => p.Name.ToLower().Equals(key)).GetValue(action.TargetUnit);
                action.TargetUnit.GetType().GetFields().ToList().Find(p => p.Name.ToLower().Equals(key)).SetValue(action.TargetUnit, stat * 2 / (-1 * value + 2));
            }
        }
        else
        {
            var damageTaken = target.TakeDamage(action.Move, action.SourceUnit);
            if (action.Move.Drain is not null)
            {
                if (action.Move.Drain.Contains(1))
                    action.SourceUnit.Heal(damageTaken / 2);
                else action.SourceUnit.Heal(damageTaken * 3 / 4);
            }
        }

        if (!target.unitName.Equals("Player"))
        {
            enemyStatus.UpdateHealthBar(target);
        }
        else
        {
            playerStatus.UpdateHealthBar(target);
        }
    }

    private void CheckCurrentBattleState()
    {
        GatherCharactersInScene();
        if (_sceneCharacters.Count == 1 && _sceneCharacters[0].unitName.Equals("Player"))
        {
            _currentState = BattleState.Win;
            _gameState.CurrentLevel += 1;
            SceneManager.LoadScene("TransitionScene");
        }
        else if (!GameObject.FindGameObjectWithTag("Player"))
        {
            _currentState = BattleState.Lost;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            BeginBattle();
        }
    }

    private void HandleAffixes(Unit character)
    {
        foreach (var affix in character.Affixes)
        {
            
        }
    }
}
