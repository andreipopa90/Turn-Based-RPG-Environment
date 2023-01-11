using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class AffixIndicator : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI affixText;
        
        public void AddAffixes(IEnumerable<string> affixes)
        {
            var text = affixes.Aggregate("", (current, affix) => current + affix + "\n");
            affixText.text = text;
        }
    }
}