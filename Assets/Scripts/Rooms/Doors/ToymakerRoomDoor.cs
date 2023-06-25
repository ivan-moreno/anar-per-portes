using AnarPerPortes.Rooms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    public class ToymakerRoomDoor : RoomDoor
    {
        [SerializeField] private AudioClip ambiance;

        public override void Open()
        {
            StartCoroutine(nameof(OpenCoroutine));
        }

        public override void Close()
        {
            if (!isOpened)
                return;

            isOpened = false;
        }

        private IEnumerator OpenCoroutine()
        {
            if (isOpened)
                yield break;

            isOpened = true;

            BlackoutManager.Singleton.PlayInstantly();
            AudioManager.Singleton.StopAmbiance();
            PlayerController.Singleton.BlockAll();
            yield return new WaitForSeconds(0.8f);

            OnDoorOpened?.Invoke();

            if (LatestRoom().Door is ToymakerRoomDoor)
            {
                BlackoutManager.Singleton.EnableSquareBars();
                PlayerController.Singleton.Teleport(transform.position + transform.forward * 2f);
                PlayerController.Singleton.UnblockAll();
                AudioManager.Singleton.PlayAmbiance(ambiance);
                RenderSettings.fog = true;
                RenderSettings.fogColor = Color.black;
                RenderSettings.fogStartDistance = 0f;
                RenderSettings.fogEndDistance = 16f;
            }
            else
            {
                BlackoutManager.Singleton.DisableSquareBars();
                PlayerController.Singleton.Teleport(transform.position + transform.forward * 2f);
                PlayerController.Singleton.UnblockAll();
                AudioManager.Singleton.StopAmbiance();
                RenderSettings.fog = false;
            }

            yield return new WaitForEndOfFrame();
            BlackoutManager.Singleton.Hide();
        }
    }
}
