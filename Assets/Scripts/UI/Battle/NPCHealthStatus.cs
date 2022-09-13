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
        private Dictionary<Unit, GameObject> _enemyPanels;

        public void SetUpEnemyStatusPanels(List<Unit> enemies)
        {
            _enemyPanels = new Dictionary<Unit, GameObject>();
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
                _enemyPanels.Add(enemies[i], enemyStatus);

                UpdateHealthBar(enemies[i]);
                UpdateDescriptionText(enemies[i]);
            
            }
        }

        private void UpdateDescriptionText(Unit enemyUnit)
        {
            var statusText = _enemyPanels[enemyUnit].GetComponentInChildren<Text>();
            statusText.text = enemyUnit.unitName + "\nLevel: " + enemyUnit.level;
        }

        public void UpdateHealthBar(Unit enemyUnit)
        {
            var healthSlider = _enemyPanels[enemyUnit].GetComponentInChildren<Slider>();
            healthSlider.maxValue = enemyUnit.maxHealth;
            healthSlider.value = Mathf.Max(0, enemyUnit.currentHealth);

            if (healthSlider.value == 0)
            {
                Destroy(_enemyPanels[enemyUnit]);
            }
        }
    }
}
