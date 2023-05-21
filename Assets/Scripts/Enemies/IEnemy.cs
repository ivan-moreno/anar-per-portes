using UnityEngine;

namespace AnarPerPortes
{
    public interface IEnemy
    {
        public string TipTitle { get; }
        public string TipMessage { get; }
        public Sprite TipRender { get; }
        public bool EnemyTipWasDisplayed { get; set; }
    }
}
