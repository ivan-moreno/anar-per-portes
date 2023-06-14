using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Game Console Manager")]
    public class GameConsoleManager : MonoBehaviour
    {
        public static GameConsoleManager Singleton { get; private set; }

        [SerializeField] private TMP_Text outputLabel;
        private readonly Queue<string> outputMessages = new(maxOutputMessages);
        private const int maxOutputMessages = 32;

        public void WriteLine(string message)
        {
            outputMessages.Enqueue(message);
            RefreshOutput();
        }

        public void Clear()
        {
            outputMessages.Clear();
            RefreshOutput();
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            outputLabel.gameObject.SetActive(CurrentSettings().EnableConsole);
        }

        private void RefreshOutput()
        {
            if (outputMessages.Count > maxOutputMessages)
                outputMessages.Dequeue();

            outputLabel.text = string.Empty;

            foreach (var message in outputMessages)
                outputLabel.text += message + "\n";
        }
    }
}
