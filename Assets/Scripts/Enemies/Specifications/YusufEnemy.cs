using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Yusuf Enemy")]
    public class YusufEnemy : Enemy
    {
        [Header("Signs")]
        [SerializeField] private Material bunkerSignMaterial;
        [SerializeField] private Material lighthouseSignMaterial;
        [SerializeField] private Material observatorySignMaterial;
        [SerializeField] private Material warehouseSignMaterial;

        [Header("Sound")]
        [SerializeField] private SoundResource walkieTalkieAlertSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource[] bunkerTargetSounds;
        [SerializeField] private SoundResource[] lighthouseTargetSounds;
        [SerializeField] private SoundResource[] observatoryTargetSounds;
        [SerializeField] private SoundResource[] warehouseTargetSounds;

        private IsleRoom isleRoom;

        private void Start()
        {
            base.Spawn();
            CacheComponents();

            isleRoom = RoomManager.Singleton.LatestRoom as IsleRoom;
            var targetPos = isleRoom.IncorrectDoor.transform.position + isleRoom.IncorrectDoor.transform.forward * 4f;
            transform.position = targetPos;
            audioSource.PlayOneShot(walkieTalkieAlertSound.AudioClip);

            var rng = Random.Range(0, 4);

            var signMaterials = new List<Material>
            {
                bunkerSignMaterial,
                lighthouseSignMaterial,
                observatorySignMaterial,
                warehouseSignMaterial
            };

            signMaterials.RemoveAt(rng);

            if (rng == 0)
            {
                audioSource.PlayOneShot(bunkerTargetSounds.RandomItem());
                isleRoom.SetSignMaterials(signMaterials.RandomItem(), bunkerSignMaterial);
            }
            else if (rng == 1)
            {
                audioSource.PlayOneShot(lighthouseTargetSounds.RandomItem());
                isleRoom.SetSignMaterials(signMaterials.RandomItem(), lighthouseSignMaterial);
            }
            else if (rng == 2)
            {
                audioSource.PlayOneShot(observatoryTargetSounds.RandomItem());
                isleRoom.SetSignMaterials(signMaterials.RandomItem(), observatorySignMaterial);
            }
            else
            {
                audioSource.PlayOneShot(warehouseTargetSounds.RandomItem());
                isleRoom.SetSignMaterials(signMaterials.RandomItem(), warehouseSignMaterial);
            }

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            isleRoom.OnDoorOpened.AddListener(Despawn);
            isleRoom.OnIncorrectDoorOpened.AddListener(CatchPlayer);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void CatchPlayer()
        {
            transform.LookAt(PlayerPosition());
            audioSource.PlayOneShot(jumpscareSound);

            if (!IsRoblomanDisguise
                && !PlayerController.Singleton.EquippedItemIs("Roblobolita"))
                PlayerController.Singleton.BeginCatchSequence();

            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            yield return new WaitForSeconds(1f);

            if (IsRoblomanDisguise)
            {
                PlayerController.Singleton.ClearVisionTarget();
                PlayerController.Singleton.UnblockAll();
                RevealRoblomanDisguise();
                Despawn();
                yield break;
            }

            if (PlayerController.Singleton.EquippedItemIs("Roblobolita"))
            {
                isleRoom.CloseIncorrectDoor();
                PlayerController.Singleton.ConsumeEquippedItem();
                BlurOverlayManager.Singleton.SetBlur(Color.white);
                BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 2f);
                yield return new WaitForSeconds(0.5f);
                PlayerController.Singleton.ClearVisionTarget();
                PlayerController.Singleton.UnblockAll();
                Despawn();
                yield break;
            }

            CatchManager.Singleton.CatchPlayer("YUSUF ENDING", "Fuerzas Yusuf, Fuerzas Yusuf");
            audioSource.Play();
        }
    }
}
