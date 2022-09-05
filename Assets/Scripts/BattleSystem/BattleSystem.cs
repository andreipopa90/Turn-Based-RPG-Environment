using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogFiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
        START, BATTLE, ACTIONRESOLVE, WIN, LOST
    }

    public GameObject Player;
    public GameObject Enemy;
    public BattleHUD MainHUD;
    public NPCHealthStatus EnemyStatus;

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
        _currentState = BattleState.START;
        _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
        _levelLog = Log.GetInstance();
        _levelLog.CurrentLevel = _gameState.CurrentLevel;
        SetUpCharacters();
        SetUpHUD();
        GatherCharactersInScene();
        BeginBattle();
    }

    private void SetUpHUD()
    {
        MainHUD.AddAbilitySelect();
        var enemies = GameObject.FindGameObjectsWithTag("Enemy")
            .Select(go => go.GetComponent<Unit>()).ToList();
        MainHUD.AddEnemySelect(enemies);
    }

    private GameObject InstantiateCharacter(string characterName, Vector3 position, GameObject prefab, bool player = false)
    {
        var character = Instantiate(prefab, position, Quaternion.identity);
        character.name = characterName;
        character.GetComponent<Unit>().UnitName = characterName;
        var characterUnit = character.GetComponent<Unit>();
        if (player)
        {
            _levelLog.PlayerDefense = characterUnit.Defense > characterUnit.MagicDefense ? "Physical" : "Special";
            _levelLog.PlayerAttack = characterUnit.Attack > characterUnit.MagicAttack ? "Physical" : "Special";
            _levelLog.PlayerStats.Add("HP", characterUnit.MaxHealth);
            _levelLog.PlayerStats.Add("ATK", characterUnit.Attack);
            _levelLog.PlayerStats.Add("DEF", characterUnit.Defense);
            _levelLog.PlayerStats.Add("SPA", characterUnit.MagicAttack);
            _levelLog.PlayerStats.Add("SPD", characterUnit.MagicDefense);
            _levelLog.PlayerStats.Add("SPE", characterUnit.Speed);
        }

        return character;
    }

    private GameObject SetUpCharacterStats(GameObject character, BaseStat characterBase, Nature characterNature)
    {
        var characterUnit = character.GetComponent<Unit>();
        characterUnit.Level = _gameState.CurrentLevel;
        characterUnit.Types = characterBase.Types;
        characterUnit.UnitNature = characterNature;
        characterUnit.SetStats(characterBase.BaseStats);
        return character;
    }

    private void SetUpCharacters()
    { 
        List<Unit> enemies = new();

        InstantiateCharacter("Player", new Vector3(0, 0, 0), Player, true);

        for (var i = 0; i < 5; i++)
        {
            var randomNumber = new System.Random().Next(0, _gameState.EnemyBaseStats.Count);
            var enemyBase = _gameState.EnemyBaseStats[randomNumber];
            AddEnemyBaseStatsToLogs(enemyBase);
            randomNumber = new System.Random().Next(0, _gameState.Natures.Count);
            var enemyNature = _gameState.Natures[randomNumber];
            var enemyInstance = InstantiateCharacter(enemyBase.Name + " " + (i + 1), 
                    new Vector3(-10 + 5 * i, 0, 10), Enemy);
            enemyInstance = SetUpCharacterStats(enemyInstance, enemyBase, enemyNature);
            enemies.Add(enemyInstance.GetComponent<Unit>());
        }
        EnemyStatus.SetUpEnemyStatusPanels(enemies);
    }

    private void AddEnemyBaseStatsToLogs(BaseStat enemyBase)
    {
        _levelLog.EnemyStats["HP"].Add(enemyBase.BaseStats["hp"]);
        _levelLog.EnemyStats["ATK"].Add(enemyBase.BaseStats["atk"]);
        _levelLog.EnemyStats["DEF"].Add(enemyBase.BaseStats["def"]);
        _levelLog.EnemyStats["SPA"].Add(enemyBase.BaseStats["spa"]);
        _levelLog.EnemyStats["SPD"].Add(enemyBase.BaseStats["spd"]);
        _levelLog.EnemyStats["SPE"].Add(enemyBase.BaseStats["spe"]);
    }

    private void GatherCharactersInScene()
    {
        _sceneCharacters = new List<Unit>();
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject)
        {
            _sceneCharacters.Add(playerObject.GetComponent<Unit>());
        }
        foreach (var go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (go)
            {
                _sceneCharacters.Add(go.GetComponent<Unit>());
            }
        }
        
    }

    private void BeginBattle()
    {
        _currentState = BattleState.BATTLE;
        PlayerTurn();
    }

    private void PlayerTurn()
    {
        MainHUD.AttackButton.gameObject.SetActive(true);
    }

    private void EnemyTurn()
    {
        while (true)
        {
            var currentEnemy = _sceneCharacters[_currentTurn];
            Action action;
            action.Move = _gameState.AllMoves[new System.Random().Next(0, 8)];
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
        if (GameObject.FindGameObjectWithTag("Player") == null || (_currentState.Equals(BattleState.BATTLE) && !_sceneCharacters[_currentTurn].name.Contains("Player")))
			return;

		if (MainHUD.EnemyPanel.activeSelf)
		    MainHUD.EnemyPanel.SetActive(false);
        MainHUD.AbilityPanel.SetActive(!MainHUD.AbilityPanel.activeSelf);
    }

    public void OnAbilityButtonPress()
    {
        var moveName = EventSystem.current.currentSelectedGameObject.name;
        _selectedMove = _gameState.SelectedMoves.Find(x => x.Name.Equals(moveName));

        MainHUD.AbilityPanel.SetActive(!MainHUD.AbilityPanel.activeSelf);
        MainHUD.EnemyPanel.SetActive(true);

    }

    public void OnEnemySelect()
	{
        Action action;
        action.Move = _selectedMove;
        action.SourceUnit = _sceneCharacters[0].GetComponent<Unit>();
        action.TargetUnit = GameObject.Find(EventSystem.current.currentSelectedGameObject.name).GetComponent<Unit>();
        _actionQueue.Add(action);

        MainHUD.EnemyPanel.SetActive(false);
        _currentTurn = (_currentTurn + 1) % _sceneCharacters.Count;

        EnemyTurn();
    }

    IEnumerator ResolveAttacks()
	{
        MainHUD.AttackButton.gameObject.SetActive(false);
        _currentState = BattleState.ACTIONRESOLVE;
        _actionQueue = _actionQueue.OrderByDescending(x => x.SourceUnit.Speed).ToList<Action>();
        
        foreach (var action in _actionQueue)
        {
            if (action.SourceUnit.UnitName.Equals("Player"))
            {
                _levelLog.PlayerMovesUsed.Add(action.Move.Name);
            }
            var target = action.TargetUnit;
            if (!action.TargetUnit && action.SourceUnit.UnitName.Equals("Player"))
            {
                var enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
                var randomEnemy = new System.Random().Next(0, enemiesArray.Length);
                if (enemiesArray.Length > 0)
                {
                    target = enemiesArray[randomEnemy].GetComponent<Unit>();
                }
            }

            if (!action.SourceUnit || !target) continue;
            target.TakeDamage(action.Move, action.SourceUnit);
            if (!target.UnitName.Equals("Player"))
            {
                EnemyStatus.UpdateHealthBar(target);
            }
            yield return new WaitForSeconds(1);

        }
        _actionQueue.Clear();
        CheckCurrentBattleState();
    }

    private void CheckCurrentBattleState()
    {
        GatherCharactersInScene();
        if (_sceneCharacters.Count == 1 && _sceneCharacters[0].UnitName.Equals("Player"))
        {
            _currentState = BattleState.WIN;
            _gameState.CurrentLevel += 1;
            SceneManager.LoadScene("TransitionScene");
        }
        else if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            _currentState = BattleState.LOST;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            BeginBattle();
        }
    }
}
