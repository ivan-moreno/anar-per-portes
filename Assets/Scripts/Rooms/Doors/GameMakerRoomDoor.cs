using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Doors/GameMaker Room Door")]
    public class GameMakerRoomDoor : RoomDoor
    {
        public override void Open()
        {
            StartCoroutine(nameof(OpenCoroutine));
        }

        public override void Close()
        {
            if (!isOpened)
                return;

            isOpened = false;
            closedCollider.enabled = true;
            transform.parent.Rotate(0f, 90f, 0f);
        }

        private IEnumerator OpenCoroutine()
        {
            if (isOpened)
                yield break;

            isOpened = true;
            closedCollider.enabled = false;
            OnDoorOpened?.Invoke();

            var timer = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * 2f;
                transform.parent.rotation = Quaternion.Lerp(Quaternion.Euler(0f, -90f, 0f), Quaternion.Euler(0f, 0f, 0f), timer);
                yield return null;
            }

            transform.parent.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
