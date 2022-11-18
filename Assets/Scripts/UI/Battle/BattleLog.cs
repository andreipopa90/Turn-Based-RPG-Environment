using TMPro;
using UnityEngine;

namespace UI.Battle
{
    public class BattleLog : MonoBehaviour
    {
        public TextMeshProUGUI TextLog;
        public GameObject LogObject;

        public void ShowMessage(string message)
        {
            LogObject.SetActive(true);
            TextLog.text = message;
        }

        public void Hide()
        {
            LogObject.SetActive(false);
        }

    }
}