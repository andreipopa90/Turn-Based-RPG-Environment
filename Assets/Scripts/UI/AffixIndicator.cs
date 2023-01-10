using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class AffixIndicator : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI affixText;
        [SerializeField] private GameObject panel;
        
        public void AddAffixes(List<string> affixes)
        {
            foreach (var affix in affixes)
            {
                affixText.text = affix + " ";
            }
        }

        public void ShowAffixes()
        {
            panel.SetActive(true);
        }

        public void HideAffixes()
        {
            panel.SetActive(false);
        }
    }
}