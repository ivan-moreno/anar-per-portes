using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Hardmode Manager")]
    public sealed class HardmodeManager : MonoBehaviour
    {
        public static HardmodeManager Singleton { get; private set; }
        public bool IsHardmodeEnabled { get; private set; } = false;

        private void Awake()
        {
            Singleton = this;
        }
    }
}
