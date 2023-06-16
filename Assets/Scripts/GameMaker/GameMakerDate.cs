using System;
using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Game Maker/Game Maker Date")]
    public class GameMakerDate : MonoBehaviour
    {
        [SerializeField] private TMP_Text dateLabel;
        private float refreshTime;

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            refreshTime += Time.unscaledDeltaTime;

            if (refreshTime >= 60f)
                Refresh();
        }

        private void Refresh()
        {
            refreshTime = 0f;

            var dateText = DateTime.Now.ToShortTimeString() + "\n";
            dateText += DateTime.Now.ToShortDateString();

            // PC_Cleaner.exe release date celebration!
            if (DateTime.Now.Day == 2
                && DateTime.Now.Month == 5)
                dateLabel.color = new Color(1f, 0f, 1f);

            dateLabel.text = dateText;
        }
    }
}
