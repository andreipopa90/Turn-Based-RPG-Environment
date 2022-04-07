using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public GameObject player;
    public GameObject enemy;

    List<Unit> sceneCharacters;
    List<Move> Moves;
    List<Action> ActionQueue;

    public BattleHUD MainHUD;

    public int currentTurn;

    private BattleState state;
    private Move SelectedMove;

    // Start is called before the first frame update
    void Start()
    {
        JSONReader reader = new();
        Moves = reader.ReadMovesJSON();
        ActionQueue = new();
        state = BattleState.START;
        SetupBattle();
    }

    void SetupBattle()
    {
        GameObject playerObject = Instantiate(player,
            new Vector3(0, 0, 0), Quaternion.identity);
        playerObject.name = "Player";
        playerObject.GetComponent<Unit>().unitName = "Player";

        for (int i = 0; i < 5; i++)
        {
            GameObject enemyObject = Instantiate(enemy,
                new Vector3(-10 + 5 * i, 0, 10), Quaternion.identity);
            enemyObject.name = "Enemy " + (i + 1);
            enemyObject.GetComponent<Unit>().unitName = "Enemy " + (i + 1);
        }

		
		List<Unit> enemies = new();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemies.Add(go.GetComponent<Unit>());
        }
        MainHUD.AddAbilitySelect();
        MainHUD.AddEnemySelect(enemies);
        BeginBattle();
    }

    void GatherCharactersInScene()
    {
        sceneCharacters = new List<Unit>
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Unit>()
        };

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (go != null)
            {
                sceneCharacters.Add(go.GetComponent<Unit>());
            }
        }
    }

    void BeginBattle()
    {
        GatherCharactersInScene();
        state = BattleState.BATTLE;
        MainHUD.AttackButton.gameObject.SetActive(true);
        PlayerTurn();
    }

    void PlayerTurn()
    {
        MainHUD.AttackButton.gameObject.SetActive(true);
    }

    void EnemyTurn()
    {
        Unit CurrentEnemy = sceneCharacters[currentTurn];
        Action action;
        action.Move = Moves[new System.Random().Next(0, 8)];
        action.SourceUnit = CurrentEnemy;
        action.TargetUnit = GameObject.Find("Player").GetComponent<Unit>();
        ActionQueue.Add(action);

        currentTurn = (currentTurn + 1) % sceneCharacters.Count;
        if (currentTurn == 0)
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
        if (GameObject.FindGameObjectWithTag("Player") == null || (state.Equals(BattleState.BATTLE) && !sceneCharacters[currentTurn].name.Contains("Player")))
		{
			return;
		}

		if (MainHUD.EnemyPanel.activeSelf)
		{
			MainHUD.EnemyPanel.SetActive(false);
		}
        MainHUD.AbilityPanel.SetActive(!MainHUD.AbilityPanel.activeSelf);
    }

    public void OnAbilityButtonPress()
    {
        string MoveName = EventSystem.current.currentSelectedGameObject.name;
        SelectedMove = Moves.Find(x => x.name.Equals(MoveName));

        MainHUD.AbilityPanel.SetActive(!MainHUD.AbilityPanel.activeSelf);
        MainHUD.EnemyPanel.SetActive(true);

    }

    public void OnEnemySelect()
	{
        Action action;
        action.Move = SelectedMove;
        action.SourceUnit = sceneCharacters[0].GetComponent<Unit>();
        action.TargetUnit = GameObject.Find(EventSystem.current.currentSelectedGameObject.name).GetComponent<Unit>();
        ActionQueue.Add(action);

        MainHUD.EnemyPanel.SetActive(false);
        currentTurn = (currentTurn + 1) % sceneCharacters.Count;

        EnemyTurn();
    }

    IEnumerator ResolveAttacks()
	{
        MainHUD.AttackButton.gameObject.SetActive(false);
        state = BattleState.ACTIONRESOLVE;
        ActionQueue = ActionQueue.OrderByDescending(x => x.SourceUnit.speed).ToList<Action>();
        
        foreach (Action action in ActionQueue)
        {
            if (action.SourceUnit.currentHealth > 0)
            {
                try
                {
                    action.TargetUnit.TakeDamage(action.Move, action.SourceUnit);
                    print(action.SourceUnit.unitName + " attacked " + action.TargetUnit.unitName);
                }
                catch (NullReferenceException)
                {
                    GameObject[] enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
                    int randomEnemy = new System.Random().Next(0, enemiesArray.Length);
                    if (enemiesArray.Length > 0)
                    {
                        Unit targetUnit = enemiesArray[randomEnemy].GetComponent<Unit>();
                        targetUnit.TakeDamage(action.Move, action.SourceUnit);
                        print(action.SourceUnit.unitName + " attacked " + targetUnit.unitName);
                    }
                    else
                    {
                        state = BattleState.WIN;
                    }
                }
                yield return new WaitForSeconds(1);
            }
        }
        ActionQueue.Clear();
        BeginBattle();
        
    }

    void CheckForLose()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            state = BattleState.LOST;
        }
    }
}
