using UnityEngine;

namespace AnarPerPortes
{
    public abstract class Enemy : MonoBehaviour
    {
        public EnemyTip Tip => tip;
        [SerializeField] private EnemyTip tip;
    }
}
