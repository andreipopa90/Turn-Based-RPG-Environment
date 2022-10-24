using System;
using Model;
using TMPro;
using UnityEditor;
using UnityEngine;
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
            backgroundTransform.sizeDelta = backgroundSize;
        }

        private void Update()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(),
                Input.mousePosition, null, out var localPoint);
            transform.localPosition = localPoint;
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}