using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Battle
{
    public class BattleHUD : MonoBehaviour
    {

        public Button EnemySelectButton;
        public Button AbilitySelectButton;
        public Button AttackButton;
        public GameObject EnemyPanel;
        public GameObject AbilityPanel;

        public void AddEnemySelect(List<Unit> enemyList)
        {
            var battleSystem = GameObject.FindGameObjectWithTag("BattleSystem").GetComponent<BattleSystem.BattleSystem>();
            for(var i = 0; i < enemyList.Count; i++)
            {
                var button = Instantiate(EnemySelectButton,
                    new Vector3(0, 0, 0), Quaternion.identity);
                button.name = enemyList[i].unitName;

                button.transform.SetParent(EnemyPanel.transform, false);
                var buttonTransform = button.GetComponent<RectTransform>();
                var buttonPosition = buttonTransform.anchoredPosition;
                buttonPosition.y = 150 - 150 * i;
                buttonTransform.anchoredPosition = buttonPosition;

                button.onClick.AddListener(battleSystem.OnEnemySelect);
                button.GetComponentInChildren<Text>().text = enemyList[i].unitName;
            }
        }

        public void AddAbilitySelect()
        {
            var playerMoves = GameObject.Find("GameState").GetComponent<GameStateStorage>().SelectedMoves;
            const int spacing = 200;
            var battleSystem = GameObject.FindGameObjectWithTag("BattleSystem").GetComponent<BattleSystem.BattleSystem>();
            for (var i = 0; i < playerMoves.Count; i++)
            {
                var button = Instantiate(AbilitySelectButton,
                    new Vector3(0, 0, 0), Quaternion.identity);
                button.name = playerMoves[i].Name;

                button.transform.SetParent(AbilityPanel.transform, false);
                var buttonTransform = button.GetComponent<RectTransform>();
                var buttonPosition = buttonTransform.anchoredPosition;
                buttonPosition.x = i < 3 ? -450 : 450;
                buttonPosition.y = i < 3 ? 200 - spacing * i : 200 - spacing * (i - 3);
                buttonTransform.anchoredPosition = buttonPosition;

                button.onClick.AddListener(battleSystem.OnAbilityButtonPress);
                button.GetComponentInChildren<Text>().text = playerMoves[i].Name;
            }
        }
    }
}

