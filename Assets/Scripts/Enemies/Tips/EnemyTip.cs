using UnityEngine;

namespace AnarPerPortes
{
    [CreateAssetMenu(menuName = "Anar per Portes/Enemy Tip")]
    public class EnemyTip : ScriptableObject
    {
        public string Title => title;
        public string Message => message;
        public Sprite Render => render;
        [SerializeField] private string title;

        [SerializeField]
        [TextArea]
        private string message;

        [SerializeField] private Sprite render;
    }
}
