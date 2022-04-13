using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{

    public Button EnemySelectButton;
    public Button AbilitySelectButton;
    public Button AttackButton;
    public GameObject EnemyPanel;
    public GameObject AbilityPanel;

    public void AddEnemySelect(List<Unit> enemyList)
    {
        int Spacing = 0;
        if (enemyList.Count == 1)
		{
            Spacing = 300;
		} else
		{
            Spacing = (int)600 / (enemyList.Count - 1);
        }
        BattleSystem battleSystem = GameObject.FindGameObjectWithTag("BattleSystem").GetComponent<BattleSystem>();
        for(int i = 0; i < enemyList.Count; i++)
        {
            Button button = Instantiate(EnemySelectButton,
                new Vector3(0, 0, 0), Quaternion.identity);
            button.name = enemyList[i].UnitName;

            button.transform.SetParent(EnemyPanel.transform, false);
            RectTransform ButtonTransform = button.GetComponent<RectTransform>();
            Vector2 ButtonPosition = ButtonTransform.anchoredPosition;
            ButtonPosition.y = 300 - Spacing * i;
            ButtonTransform.anchoredPosition = ButtonPosition;

            button.onClick.AddListener(battleSystem.OnEnemySelect);
            button.GetComponentInChildren<Text>().text = enemyList[i].UnitName;
        }
    }

    public void AddAbilitySelect()
    {
        List<Move> playerMoves = GameObject.Find("MoveSelection").GetComponent<MoveSelection>().SelectedMoves;
        int Spacing = 200;
        BattleSystem battleSystem = GameObject.FindGameObjectWithTag("BattleSystem").GetComponent<BattleSystem>();
        for (int i = 0; i < playerMoves.Count; i++)
        {
            Button button = Instantiate(AbilitySelectButton,
                new Vector3(0, 0, 0), Quaternion.identity);
            button.name = playerMoves[i].Name;

            button.transform.SetParent(AbilityPanel.transform, false);
            RectTransform ButtonTransform = button.GetComponent<RectTransform>();
            Vector2 ButtonPosition = ButtonTransform.anchoredPosition;
            ButtonPosition.x = (i >= 4) ? 450 : -450;
            ButtonPosition.y = (i < 4) ? (300 - Spacing * i) : (300 - Spacing * (i - 4));
            ButtonTransform.anchoredPosition = ButtonPosition;

            button.onClick.AddListener(battleSystem.OnAbilityButtonPress);
            button.GetComponentInChildren<Text>().text = playerMoves[i].Name;
        }
    }
}

