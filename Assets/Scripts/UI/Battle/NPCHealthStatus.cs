using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Battle
{
    public class NPCHealthStatus : MonoBehaviour
    {

        public GameObject EnemyStatusPanel;
        public GameObject StatusPanel;
        private Dictionary<Unit, GameObject> _panels;

        public void SetUpEnemyStatusPanels(List<Unit> enemies)
        {
            _panels = new Dictionary<Unit, GameObject>();
            for (var i = 0; i < 3; i++)
            {
                var enemyStatus = Instantiate(EnemyStatusPanel, 
                    new Vector3(0, 0, 0), Quaternion.identity);
                enemyStatus.transform.SetParent(StatusPanel.transform, false);
                var statusPanelTransform = enemyStatus.GetComponent<RectTransform>();
                var statusPanelPosition = statusPanelTransform.anchoredPosition;
                statusPanelPosition.x = -200 + i * 200;
                statusPanelPosition.y = 125;
                statusPanelTransform.anchoredPosition = statusPanelPosition;
                _panels.Add(enemies[i], enemyStatus);

                UpdateHealthBar(enemies[i]);
                UpdateDescriptionText(enemies[i]);
            
            }
        }

        public void SetUpPlayerStatusPanels(Unit player)
        {
            var playerStatus = Instantiate(EnemyStatusPanel, 
                new Vector3(0, 0, 0), Quaternion.identity);
            playerStatus.transform.SetParent(StatusPanel.transform, false);
            var statusPanelTransform = playerStatus.GetComponent<RectTransform>();
            var statusPanelPosition = statusPanelTransform.anchoredPosition;
            statusPanelPosition.x = 0;
            statusPanelPosition.y = -550;
            statusPanelTransform.anchoredPosition = statusPanelPosition;
            _panels.Add(player, playerStatus);
            UpdateHealthBar(player);
            UpdateDescriptionText(player);
        }

        private void UpdateDescriptionText(Unit characterUnit)
        {
            var statusText = _panels[characterUnit].GetComponentInChildren<Text>();
            statusText.text = characterUnit.unitName + "\nLevel: " + characterUnit.level;
        }

        public void UpdateHealthBar(Unit characterUnit)
        {
            var healthSlider = _panels[characterUnit].GetComponentInChildren<Slider>();
            healthSlider.maxValue = characterUnit.maxHealth;
            healthSlider.value = Mathf.Max(0, characterUnit.currentHealth);

            if (healthSlider.value == 0)
            {
                Destroy(_panels[characterUnit]);
            }
        }
    }
}
