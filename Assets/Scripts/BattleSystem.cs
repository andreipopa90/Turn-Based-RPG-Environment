using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
    private enum BattleState
    {
        START, BATTLE, ACTIONRESOLVE, WIN, LOST
    }
    public GameObject player;
    public GameObject enemy;

    List<Unit> sceneCharacters;
    List<Attack> AttackQueue;
    List<Move> Moves;

    public BattleHUD MainHUD;

    public int currentTurn;

    private BattleState state;

    // Start is called before the first frame update
    void Start()
    {
        JSONReader reader = new();
        Moves = reader.ReadMovesJSON();
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
        AttackQueue = new();
        MainHUD.AddAbilitySelect(Moves.GetRange(0, 8));
        MainHUD.AddEnemySelect(enemies);
        BeginBattle();
    }

    private void Update()
    {
        GatherCharactersInScene();
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
        print("Number of total characters in scene is: " + sceneCharacters.Count);
        PlayerTurn();
    }

    void PlayerTurn()
    {
        MainHUD.AttackButton.gameObject.SetActive(true);
    }

    void EnemyTurn()
    {

        Attack attack = new(Moves[0], sceneCharacters[currentTurn].speed, sceneCharacters[currentTurn].level, sceneCharacters[currentTurn].attack);
        attack.SetTarget(GameObject.Find("Player").name);
        AttackQueue.Add(attack);

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
        //if (GameObject.FindGameObjectWithTag("Player") == null)
        //{
        //    print("No player!");
        //    return;
        //}
        if (state.Equals(BattleState.BATTLE) && !sceneCharacters[currentTurn].name.Contains("Player"))
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
        Move usedMove = null;
        foreach(Move move in Moves)
        {
            if (move.name.Equals(MoveName))
            {
                usedMove = move;
                break;
            }
        }
        AttackQueue.Add(new Attack(usedMove, GameObject.Find("Player").GetComponent<Unit>().speed, GameObject.Find("Player").GetComponent<Unit>().level, GameObject.Find("Player").GetComponent<Unit>().attack));
        MainHUD.AbilityPanel.SetActive(!MainHUD.AbilityPanel.activeSelf);
        MainHUD.EnemyPanel.SetActive(true);

    }

    public void OnEnemySelect()
	{
        Attack attack = AttackQueue[0];
        attack.SetTarget(EventSystem.current.currentSelectedGameObject.name);

        MainHUD.EnemyPanel.SetActive(false);
        currentTurn = (currentTurn + 1) % sceneCharacters.Count;

        EnemyTurn();
    }

    IEnumerator ResolveAttacks()
	{
        MainHUD.AttackButton.gameObject.SetActive(false);
        state = BattleState.ACTIONRESOLVE;
        AttackQueue = AttackQueue.OrderByDescending(x => x.Speed).ToList();
        foreach(Attack attack in AttackQueue)
		{
            try
            {
                Unit targetUnit = GameObject.Find(attack.GetTarget()).GetComponent<Unit>();
                targetUnit.TakeDamage(attack.Level, attack.Move.basePower, attack.UnitAttack, false);
            } catch (NullReferenceException)
            {
                GameObject[] enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
                System.Random rnd = new System.Random();
                int randomEnemy = rnd.Next(0, enemiesArray.Length);
                if (enemiesArray.Length > 0)
                {
                    Unit targetUnit = enemiesArray[randomEnemy].GetComponent<Unit>();
                    targetUnit.TakeDamage(attack.Level, attack.Move.basePower, attack.UnitAttack, false);
                } else
                {
                    state = BattleState.WIN;
                }
            }
        }
        yield return new WaitForSeconds(2f);
        AttackQueue.Clear();
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
