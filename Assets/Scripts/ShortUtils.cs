using UnityEngine;

namespace AnarPerPortes
{
    public static class ShortUtils
    {
        public static bool IsHardmodeEnabled()
        {
            return HardmodeManager.Singleton.IsHardmodeEnabled;
        }

        public static Vector3 PlayerPosition()
        {
            return PlayerController.Singleton.transform.position;
        }
    }
}
