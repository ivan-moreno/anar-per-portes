using UnityEngine;

namespace AnarPerPortes
{
    public abstract class Enemy : MonoBehaviour, IEnemy
    {
        public abstract bool EnemyTipWasDisplayed { get; set; }
        public string TipTitle => tipTitle;
        public string TipMessage => tipMessage;
        public Sprite TipRender => tipRender;

        [SerializeField]
        private string tipTitle;

        [SerializeField]
        [TextArea]
        private string tipMessage;

        [SerializeField]
        private Sprite tipRender;
    }
}
