using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveSelection : MonoBehaviour
{

    public List<Move> SelectedMoves;

    // Start is called before the first frame update
    void Start()
    {
        SelectedMoves = new();
    }

    public void OnPressLockIn()
    {
        if (SelectedMoves.Count == 8)
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(GameObject.Find("MoveSelection"));
    }


}
