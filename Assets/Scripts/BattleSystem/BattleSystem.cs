using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private int CurrentTurn;
    private List<Unit> SceneCharacters;
    private List<Action> ActionQueue;
    private BattleState CurrentState;
    private Move SelectedMove;
    private GameStateStorage GameState;

    // Start is called before the first frame update
    void Start()
    {
        ActionQueue = new();
        CurrentState = BattleState.START;
        GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
        SetUpCharacters();
        SetUpHUD();
        GatherCharactersInScene();
        BeginBattle();
    }

    void SetUpHUD()
    {
        MainHUD.AddAbilitySelect();
        List<Unit> Enemies = new();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Enemies.Add(go.GetComponent<Unit>());
        }
        MainHUD.AddEnemySelect(Enemies);
    }

    GameObject InstantiateCharacter(string name, Vector3 position, GameObject prefab)
    {
        GameObject Character = Instantiate(prefab, position, Quaternion.identity);
        Character.name = name;
        Character.GetComponent<Unit>().UnitName = name;
        return Character;
    }

    GameObject SetUpCharacterStats(GameObject Character, BaseStat CharacterBase, Nature CharacterNature)
    {
        Unit CharacterUnit = Character.GetComponent<Unit>();
        CharacterUnit.Level = GameState.CurrentLevel;
        CharacterUnit.Types = CharacterBase.Types;
        CharacterUnit.UnitNature = CharacterNature;
        CharacterUnit.SetStats(CharacterBase.BaseStats);
        return Character;
    }

    void SetUpCharacters()
    { 
        List<Unit> Enemies = new();

        InstantiateCharacter("Player", new Vector3(0, 0, 0), Player);

        for (int i = 0; i < 5; i++)
        {
            int RandomNumber = new System.Random().Next(0, GameState.EnemyBaseStats.Count);
            BaseStat EnemyBase = GameState.EnemyBaseStats[RandomNumber];
            RandomNumber = new System.Random().Next(0, GameState.Natures.Count);
            Nature EnemyNature = GameState.Natures[RandomNumber];
            GameObject EnemyInstance = InstantiateCharacter(EnemyBase.Name + " " + (i + 1), new Vector3(-10 + 5 * i, 0, 10), Enemy);
            EnemyInstance = SetUpCharacterStats(EnemyInstance, EnemyBase, EnemyNature);
            Enemies.Add(EnemyInstance.GetComponent<Unit>());
        }
        EnemyStatus.SetUpEnemyStatusPanels(Enemies);
    }

    void GatherCharactersInScene()
    {
        SceneCharacters = new();
        GameObject PlayerObject = GameObject.FindGameObjectWithTag("Player");
        if (PlayerObject != null)
        {
            SceneCharacters.Add(PlayerObject.GetComponent<Unit>());
        }
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (go != null)
            {
                SceneCharacters.Add(go.GetComponent<Unit>());
            }
        }
        
    }

    void BeginBattle()
    {
        CurrentState = BattleState.BATTLE;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        MainHUD.AttackButton.gameObject.SetActive(true);
    }

    void EnemyTurn()
    {
        Unit CurrentEnemy = SceneCharacters[CurrentTurn];
        Action Action;
        Action.Move = GameState.AllMoves[new System.Random().Next(0, 8)];
        Action.SourceUnit = CurrentEnemy;
        Action.TargetUnit = GameObject.Find("Player").GetComponent<Unit>();
        ActionQueue.Add(Action);

        CurrentTurn = (CurrentTurn + 1) % SceneCharacters.Count;
        if (CurrentTurn == 0)
		{
            StartCoroutine(ResolveAttacks());
        }
        else
        {
            EnemyTurn();
        }
    }

    public void OnAttackButtonPress()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null || (CurrentState.Equals(BattleState.BATTLE) && !SceneCharacters[CurrentTurn].name.Contains("Player")))
			return;

		if (MainHUD.EnemyPanel.activeSelf)
		    MainHUD.EnemyPanel.SetActive(false);
        MainHUD.AbilityPanel.SetActive(!MainHUD.AbilityPanel.activeSelf);
    }

    public void OnAbilityButtonPress()
    {
        string MoveName = EventSystem.current.currentSelectedGameObject.name;
        SelectedMove = GameState.SelectedMoves.Find(x => x.Name.Equals(MoveName));

        MainHUD.AbilityPanel.SetActive(!MainHUD.AbilityPanel.activeSelf);
        MainHUD.EnemyPanel.SetActive(true);

    }

    public void OnEnemySelect()
	{
        Action Action;
        Action.Move = SelectedMove;
        Action.SourceUnit = SceneCharacters[0].GetComponent<Unit>();
        Action.TargetUnit = GameObject.Find(EventSystem.current.currentSelectedGameObject.name).GetComponent<Unit>();
        ActionQueue.Add(Action);

        MainHUD.EnemyPanel.SetActive(false);
        CurrentTurn = (CurrentTurn + 1) % SceneCharacters.Count;

        EnemyTurn();
    }

    IEnumerator ResolveAttacks()
	{
        MainHUD.AttackButton.gameObject.SetActive(false);
        CurrentState = BattleState.ACTIONRESOLVE;
        ActionQueue = ActionQueue.OrderByDescending(x => x.SourceUnit.Speed).ToList<Action>();
        
        foreach (Action Action in ActionQueue)
        {
            Unit Target = Action.TargetUnit;
            if (Action.TargetUnit == null && Action.SourceUnit.UnitName.Equals("Player"))
            {
                GameObject[] enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
                int randomEnemy = new System.Random().Next(0, enemiesArray.Length);
                if (enemiesArray.Length > 0)
                {
                    Target = enemiesArray[randomEnemy].GetComponent<Unit>();
                }
            }
            if (Action.SourceUnit != null && Target != null)
            {
                Target.TakeDamage(Action.Move, Action.SourceUnit);
                if (!Target.UnitName.Equals("Player"))
                {
                    EnemyStatus.UpdateHealthBar(Target);
                }
                yield return new WaitForSeconds(1);
            }

        }
        ActionQueue.Clear();
        CheckCurrentBattleState();
    }

    void CheckCurrentBattleState()
    {
        GatherCharactersInScene();
        if (SceneCharacters.Count == 1 && SceneCharacters[0].UnitName.Equals("Player"))
        {
            CurrentState = BattleState.WIN;
            GameState.CurrentLevel += 1;
            SceneManager.LoadScene("TransitionScene");
        }
        else if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            CurrentState = BattleState.LOST;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            BeginBattle();
        }
    }
}
