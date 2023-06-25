using UnityEngine;

namespace AnarPerPortes.Rooms
{
    public class BouserBossRoom : Room
    {
        [SerializeField] private Transform playerStartPoint;

        public override void Initialize()
        {
            base.Initialize();
            PlayerController.Singleton.Teleport(playerStartPoint.position);
        }
    }
}
