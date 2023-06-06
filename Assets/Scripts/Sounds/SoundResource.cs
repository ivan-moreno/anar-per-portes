using UnityEngine;

namespace AnarPerPortes
{
    [CreateAssetMenu(menuName = "Anar per Portes/Sound Resource")]
    public class SoundResource : ScriptableObject
    {
        public AudioClip AudioClip => audioClip;
        public string SubtitleText => subtitleText;
        public Team SubtitleTeam => subtitleTeam;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private string subtitleText;
        [SerializeField] private Team subtitleTeam;
    }
}
