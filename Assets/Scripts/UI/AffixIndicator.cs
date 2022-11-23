using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class AffixIndicator : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI affixText;
        
        public void AddAffixes(List<string> affixes)
        {
            foreach (var affix in affixes)
            {
                affixText.text = affix + " ";
            }
        }
    }
}