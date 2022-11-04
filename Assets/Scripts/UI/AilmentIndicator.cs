using UnityEngine;

namespace UI
{
    public class AilmentIndicator : MonoBehaviour
    {

        public GameObject paralysis;
        public GameObject burn;
        public GameObject poison;

        public void ShowAilment(string ailment)
        {
            switch (ailment)
            {
                case "paralysis":
                case "par":
                    paralysis.SetActive(true);
                    break;
                case "burn":
                case "brn":
                    burn.SetActive(true);
                    break;
                case "poison":
                case "psn":
                    poison.SetActive(true);
                    break;
                default:
                    print("No ailment with this name");
                    break;
            }
        }

        public void HideAilments()
        {
            paralysis.SetActive(false);
            burn.SetActive(false);
            poison.SetActive(false);
        }

    }
}