using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateStorage : MonoBehaviour
{

    public List<Move> SelectedMoves;
    public int CurrentLevel { get; set; }
    public List<BaseStat> EnemyBaseStats { get; set; }
    public List<Type> TypeChart { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        SelectedMoves = new();
        CurrentLevel = 1;
        JSONReader reader = new();
        EnemyBaseStats = reader.ReadBaseStatsJSON();
        TypeChart = reader.ReadTypeChart();
    }

    public void OnPressLockIn()
    {
        if (SelectedMoves.Count == 8)
        {
            SceneManager.LoadScene("BattleScene");
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(GameObject.Find("GameState"));
    }


}
