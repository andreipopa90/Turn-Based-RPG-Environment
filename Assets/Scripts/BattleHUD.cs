using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{

    public Text nameText;
    public Text levelText;
    public Slider hpSlider;
    public Button EnemySelectButton;
    public GameObject EnemyPanel;

    public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;
        levelText.text = "Lvl " + unit.level;
        hpSlider.maxValue = unit.maxHealth;
        hpSlider.value = unit.currentHealth;
    }

    public void SetHealth(int health)
    {
        hpSlider.value = health;
    }

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
            button.name = enemyList[i].unitName;

            button.transform.SetParent(EnemyPanel.transform, false);
            RectTransform ButtonTransform = button.GetComponent<RectTransform>();
            Vector2 ButtonPosition = ButtonTransform.anchoredPosition;
            ButtonPosition.y = -300 + Spacing * i;
            ButtonTransform.anchoredPosition = ButtonPosition;

            button.onClick.AddListener(battleSystem.OnEnemySelect);
        }
    }
}

