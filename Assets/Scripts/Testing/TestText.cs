using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Testing
{
    public class TestText : MonoBehaviour
    {

        public TextMeshProUGUI textToShow;
        public Image backgroundColor;

        private List<Color32> _colors = new()
        {
            Color.cyan, Color.blue, Color.white, Color.magenta, Color.red
        };
        
        private void Start()
        {
            textToShow.text = "Welcome to my slide show!";
            StartCoroutine(SlideShowCoroutine());
        }


        private IEnumerator SlideShowCoroutine()
        {
            for (var i = 0; i < 4; i++)
            {
                // ShowSlides();
                yield return ShowSlides();
            }
        }

        private IEnumerator ShowSlides()
        {
            var pickedColor = _colors[new System.Random().Next(maxValue: _colors.Count)];
            _colors.Remove(pickedColor);
            backgroundColor.color = pickedColor;
            var timesToChangeTheText = new System.Random().Next(minValue: 2, maxValue: 6);
            for (var i = 0; i < timesToChangeTheText; i++)
            {
                yield return ChangeText(i);
            }
        }

        private IEnumerator ChangeText(int time)
        {
            print("Text changes: " + time);
            textToShow.text = "Text changes: " + time;
            yield return new WaitForSeconds(1f);
        }
    }
}