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
        private Dictionary<Unit, GameObject> EnemyPanels;

        public void SetUpEnemyStatusPanels(List<Unit> Enemies)
        {
            EnemyPanels = new();
            for (int i = 0; i < 5; i++)
            {
                GameObject EnemyStatus = Instantiate(EnemyStatusPanel, new Vector3(0, 0, 0), Quaternion.identity);
                EnemyStatus.transform.SetParent(StatusPanel.transform, false);
                RectTransform StatusPanelTransform = EnemyStatus.GetComponent<RectTransform>();
                Vector2 StatusPanelPosition = StatusPanelTransform.anchoredPosition;
                StatusPanelPosition.x = -320 + i * 160;
                StatusPanelPosition.y = 125;
                StatusPanelTransform.anchoredPosition = StatusPanelPosition;
                EnemyPanels.Add(Enemies[i], EnemyStatus);

                UpdateHealthBar(Enemies[i]);
                UpdateDescriptionText(Enemies[i]);
            
            }
        }

        void UpdateDescriptionText(Unit EnemyUnit)
        {
            Text StatusText = EnemyPanels[EnemyUnit].GetComponentInChildren<Text>();
            StatusText.text = EnemyUnit.unitName + "\nLevel: " + EnemyUnit.level;
        }

        public void UpdateHealthBar(Unit EnemyUnit)
        {
            Slider HealthSlider = EnemyPanels[EnemyUnit].GetComponentInChildren<Slider>();
            HealthSlider.maxValue = EnemyUnit.maxHealth;
            HealthSlider.value = Mathf.Max(0, EnemyUnit.currentHealth);

            if (HealthSlider.value == 0)
            {
                Destroy(EnemyPanels[EnemyUnit]);
            }
        }
    }
}
