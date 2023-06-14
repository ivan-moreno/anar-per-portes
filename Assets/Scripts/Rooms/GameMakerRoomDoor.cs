using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Doors/GameMaker Room Door")]
    public class GameMakerRoomDoor : RoomDoor
    {
        public override void Open()
        {
            if (isOpened)
                return;

            isOpened = true;
            closedCollider.enabled = false;
            transform.parent.Rotate(0f, -90f, 0f);
        }

        public override void Close()
        {
            if (!isOpened)
                return;

            isOpened = false;
            closedCollider.enabled = true;
            transform.parent.Rotate(0f, 90f, 0f);
        }
    }
}
