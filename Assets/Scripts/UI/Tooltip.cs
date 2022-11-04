using System;
using Model;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        public TextMeshProUGUI tooltipText;
        public RectTransform backgroundTransform;
        public TextMeshProUGUI moveName;

        private GameStateStorage GameState { get; set; }

        public void ShowTooltip()
        {
            GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            var move = GameState.AllMoves.Find(m => m.Name.Equals(moveName.text));
            tooltipText.text = move.ToString();
            gameObject.SetActive(true);
            const float paddingSize = 8f;
            var backgroundSize = new Vector2(tooltipText.preferredWidth + paddingSize, 
                tooltipText.preferredHeight + paddingSize);
            var transform1 = transform;
            var position = transform1.localPosition;
            float y;
            float x;
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                x = position.x;
                y = 0;
            }
            else
            {
                x = position.x;
                y = position.y == 0 ? position.y - tooltipText.preferredHeight : position.y;
            }

            transform.localPosition = new Vector3(x, y, position.z);
            backgroundTransform.sizeDelta = backgroundSize;
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}