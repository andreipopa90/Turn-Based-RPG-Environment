using System.Collections.Generic;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Battle
{
    public class NPCHealthStatus : MonoBehaviour
    {

        [SerializeField] private GameObject characterStatusPanel;
        [SerializeField] private GameObject statusPanel;
        private Dictionary<Unit, GameObject> _panels;

        public Dictionary<Unit, GameObject> GetPanels()
        {
            return _panels;
        }

        public void SetUpEnemyStatusPanels(List<Unit> enemies)
        {
            _panels = new Dictionary<Unit, GameObject>();
            for (var i = 0; i < GameStateStorage.EnemyCount; i++)
            {
                var enemyStatus = Instantiate(characterStatusPanel, 
                    new Vector3(0, 0, 0), Quaternion.identity);
                enemyStatus.transform.SetParent(statusPanel.transform, false);
                var statusPanelTransform = enemyStatus.GetComponent<RectTransform>();
                var statusPanelPosition = statusPanelTransform.anchoredPosition;
                statusPanelPosition.x = -200 + i * 400;
                statusPanelPosition.y = 125;
                statusPanelTransform.anchoredPosition = statusPanelPosition;
                _panels.Add(enemies[i], enemyStatus);

                UpdateHealthBar(enemies[i]);
                UpdateDescriptionText(enemies[i]);
                AddAffixes(enemies[i]);
            }
        }

        private void AddAffixes(Unit enemy)
        {
            _panels[enemy].GetComponent<AffixIndicator>().AddAffixes(enemy.Affixes);
        }

        public void SetUpPlayerStatusPanels(Unit player)
        {
            var playerStatus = Instantiate(characterStatusPanel, 
                new Vector3(0, 0, 0), Quaternion.identity);
            playerStatus.transform.SetParent(statusPanel.transform, false);
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
            var statusText = _panels[characterUnit].GetComponentInChildren<TextMeshProUGUI>();
            statusText.text = characterUnit.ToString();
        }

        public void UpdateHealthBar(Unit characterUnit)
        {
            var healthSlider = _panels[characterUnit].GetComponentInChildren<Slider>();
            healthSlider.maxValue = characterUnit.MaxHealth;
            healthSlider.value = Mathf.Max(0, characterUnit.CurrentHealth);

            if (healthSlider.value == 0)
            {
                Destroy(_panels[characterUnit]);
            }
        }

        public void UpdateHealthBar(List<Unit> characterUnits)
        {
            foreach (var character in characterUnits)
            {
                UpdateHealthBar(character);
            }
        }
    }
}
