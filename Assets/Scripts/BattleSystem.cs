using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState
{
    START, BATTLE, WON, LOST
}

public class BattleSystem : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;

    List<Unit> sceneCharacters;

    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    public BattleState battleState;

    public int currentTurn;

    // Start is called before the first frame update
    void Start()
    {
        battleState = BattleState.START;
        SetupBattle();
    }

    void SetupBattle()
    {
        GameObject playerObject = Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);

        for (int i = 0; i < 5; i++)
        {
            GameObject enemyObject = Instantiate(enemy, new Vector3(-10 + 5 * i, 0, 10), Quaternion.identity);
        }

        battleState = BattleState.BATTLE;
        sceneCharacters = new List<Unit>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            sceneCharacters.Add(go.GetComponent<Unit>());
        }
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            sceneCharacters.Add(go.GetComponent<Unit>());
        }
        sceneCharacters = sceneCharacters.OrderByDescending(x => x.speed).ToList();
        currentTurn = 0;
        BeginBattle();
    }

    void BeginBattle()
    {
        
        print(sceneCharacters.Count);
        //if (sceneCharacters[currentTurn].Equals("Player"))
        //{
        //    PlayerTurn();
        //} else
        //{
        //    EnemyTurn();
        //}

    }

    //void PlayerTurn()
    //{
    //    currentTurn = (currentTurn + 1) % sceneCharacters.Count;
    //    if (sceneCharacters[currentTurn].Equals("Player"))
    //    {
    //        PlayerTurn();
    //    }
    //    else
    //    {
    //        EnemyTurn();
    //    }
    //}

    //void EnemyTurn()
    //{

    //    print(sceneCharacters[currentTurn].currentHealth);

    //    currentTurn = (currentTurn + 1) % sceneCharacters.Count;
    //    if (sceneCharacters[currentTurn].Equals("Player"))
    //    {
    //        PlayerTurn();
    //    }
    //    else
    //    {
    //        EnemyTurn();
    //    }
    //}
}
