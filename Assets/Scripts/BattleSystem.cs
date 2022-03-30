using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleSystem : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;

    List<Unit> sceneCharacters;
    List<Attack> AttackQueue;

    public BattleHUD MainHUD;

    public GameObject AbilityPanel;

    public int currentTurn;

    // Start is called before the first frame update
    void Start()
    {
        JSONReader reader = new();
        reader.ReadAbilityJSON();
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

		sceneCharacters = new List<Unit>
		{
			GameObject.FindGameObjectWithTag("Player").GetComponent<Unit>()
		};
		List<Unit> enemies = new();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemies.Add(go.GetComponent<Unit>());
        }
        sceneCharacters.AddRange(enemies);

        currentTurn = 0;
        AttackQueue = new();
        MainHUD.AddEnemySelect(enemies);
        BeginBattle();
    }

    void BeginBattle()
    {
        PlayerTurn();
    }

    void PlayerTurn()
    {
        print("It is your turn!");
    }

    void EnemyTurn()
    {

        Attack attack = new(10, 10, 10, 10);
        attack.SetTarget(GameObject.Find("Player").name);
        AttackQueue.Add(attack);

        currentTurn = (currentTurn + 1) % sceneCharacters.Count;
        if (currentTurn == 0)
		{
            ResolveAttacks();
		}
        if (sceneCharacters[currentTurn].name.Contains("Player"))
        {
            PlayerTurn();
        }
        else
        {
            EnemyTurn();
        }
    }

    public void OnAttackButtonPress()
    {
        if (!sceneCharacters[currentTurn].name.Contains("Player"))
		{
			return;
		}

		if (MainHUD.EnemyPanel.activeSelf)
		{
			MainHUD.EnemyPanel.SetActive(false);
		}
        AbilityPanel.SetActive(!AbilityPanel.activeSelf);
    }

    public void OnAbilityButtonPress()
    {
        AbilityPanel.SetActive(!AbilityPanel.activeSelf);
        MainHUD.EnemyPanel.SetActive(true);
        SetAbility(EventSystem.current.currentSelectedGameObject.name);
        
    }

    public void OnEnemySelect()
	{
        Attack attack = AttackQueue[0];
        attack.SetTarget(EventSystem.current.currentSelectedGameObject.name);

        MainHUD.EnemyPanel.SetActive(false);
        currentTurn = (currentTurn + 1) % sceneCharacters.Count;

        if (currentTurn == 0)
        {
            ResolveAttacks();
        }
        if (sceneCharacters[currentTurn].name.Contains("Player"))
        {
            PlayerTurn();
        }
        else
        {
            EnemyTurn();
        }
    }

    void SetAbility(string AbilityName)
	{
        Attack attack = new(10, 10, 10, 100);
        AttackQueue.Add(attack);
	}

    void ResolveAttacks()
	{
        AttackQueue = AttackQueue.OrderByDescending(x => x.Speed).ToList();
        foreach(Attack attack in AttackQueue)
		{
            Unit targetUnit = GameObject.Find(attack.GetTarget()).GetComponent<Unit>();
            targetUnit.TakeDamage(attack.Level, attack.AttackPower, attack.Power, false);
		}
        AttackQueue.Clear();
	}
}
