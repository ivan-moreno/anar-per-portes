using UnityEngine;

namespace AnarPerPortes
{
    public static class ShortUtils
    {
        public static bool IsHardmodeEnabled()
        {
            return HardmodeManager.Singleton.IsHardmodeEnabled;
        }

        public static void PlayerSound(SoundResource sound)
        {
            PlayerController.Singleton.PlaySound(sound);
        }

        public static Vector3 PlayerPosition()
        {
            return PlayerController.Singleton.transform.position;
        }

        public static float DistanceToPlayer(Transform originTransform)
        {
            return Vector3.Distance(originTransform.position, PlayerPosition());
        }

        public static float DistanceToPlayer(Vector3 origin)
        {
            return Vector3.Distance(origin, PlayerPosition());
        }

        public static bool PlayerIsInLineOfSight(Vector3 sightOrigin)
        {
            return Physics.Linecast(
                    start: sightOrigin,
                    end: PlayerPosition() + Vector3.up,
                    hitInfo: out var hit,
                    layerMask: LayerMask.GetMask("Default", "Player"),
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        public static bool TryConsumePlayerImmunityItem()
        {
            if (!PlayerController.Singleton.EquippedItemIs("Roblobolita"))
                return false;

            PlayerController.Singleton.ConsumeEquippedItem();
            BlurOverlayManager.Singleton.SetBlur(Color.white);
            BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 2f);
            return true;
        }

        public static void PushSubtitle(SoundResource soundResource)
        {
            SubtitleManager.Singleton.PushSubtitle(soundResource);
        }

        public static void PushSubtitle(string message, float duration = 4f, Team team = Team.Common)
        {
            SubtitleManager.Singleton.PushSubtitle(message, duration, team);
        }
    }
}
