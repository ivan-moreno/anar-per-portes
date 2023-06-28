using AnarPerPortes.Rooms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Yusuf Enemy")]
    public class YusufEnemy : Enemy
    {
        [Header("Stats")]
        [SerializeField][Range(0f, 100f)] private float correctChance = 80f;
        [SerializeField][Min(0f)] private float waitToCorrectDuration = 4f;

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
        [SerializeField] private SoundResource correctBunkerSound;
        [SerializeField] private SoundResource correctLighthouseSound;
        [SerializeField] private SoundResource correctObservatorySound;
        [SerializeField] private SoundResource correctWarehouseSound;

        private IsleRoom isleRoom;
        private bool sayIncorrect = false;
        private int locationRng;
        private static bool showedIntro = false;

        private void Start()
        {
            base.Spawn();
            CacheComponents();

            isleRoom = RoomManager.Singleton.LatestRoom as IsleRoom;
            var targetPos = isleRoom.IncorrectDoor.transform.position + (isleRoom.IncorrectDoor.transform.forward * 4f);
            transform.position = targetPos;
            audioSource.PlayOneShot(walkieTalkieAlertSound.AudioClip);

            PrepareRoom();

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            isleRoom.OnDoorOpened.AddListener(Despawn);
            isleRoom.OnIncorrectDoorOpened.AddListener(CatchPlayer);
            BouserBossEnemy.OnSpawn.AddListener((_) => Despawn());
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            if (!showedIntro && !isInIntro && CurrentSettings().EnableEnemyTips)
            {
                showedIntro = true;
                StartCoroutine(nameof(IntroCinematicCoroutine));
            }
        }

        private void PrepareRoom()
        {
            locationRng = Random.Range(0, 4);
            var correctRng = Random.Range(0f, 100f);

            sayIncorrect = correctRng > correctChance;

            var signMaterials = new List<Material>
            {
                bunkerSignMaterial,
                lighthouseSignMaterial,
                observatorySignMaterial,
                warehouseSignMaterial
            };

            signMaterials.RemoveAt(locationRng);

            if (locationRng == 0)
            {
                if (sayIncorrect)
                    StartCoroutine(nameof(CorrectCoroutine));
                else
                    audioSource.PlayOneShot(bunkerTargetSounds.RandomItem());

                isleRoom.SetSignMaterials(signMaterials.RandomItem(), bunkerSignMaterial);
            }
            else if (locationRng == 1)
            {
                if (sayIncorrect)
                    StartCoroutine(nameof(CorrectCoroutine));
                else
                    audioSource.PlayOneShot(lighthouseTargetSounds.RandomItem());

                isleRoom.SetSignMaterials(signMaterials.RandomItem(), lighthouseSignMaterial);
            }
            else if (locationRng == 2)
            {
                if (sayIncorrect)
                    StartCoroutine(nameof(CorrectCoroutine));
                else
                    audioSource.PlayOneShot(observatoryTargetSounds.RandomItem());

                isleRoom.SetSignMaterials(signMaterials.RandomItem(), observatorySignMaterial);
            }
            else
            {
                if (sayIncorrect)
                    StartCoroutine(nameof(CorrectCoroutine));
                else
                    audioSource.PlayOneShot(warehouseTargetSounds.RandomItem());

                isleRoom.SetSignMaterials(signMaterials.RandomItem(), warehouseSignMaterial);
            }
        }

        protected override void Despawn()
        {
            if (isCatching)
                return;

            PlayerCollectTix(10, "Has evadido a Yusuf");
            base.Despawn();
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private IEnumerator CorrectCoroutine()
        {
            var soundsPool = new List<SoundResource>();

            if (locationRng == 0)
            {
                soundsPool.AddRange(lighthouseTargetSounds);
                soundsPool.AddRange(observatoryTargetSounds);
                soundsPool.AddRange(warehouseTargetSounds);
            }
            else if (locationRng == 1)
            {
                soundsPool.AddRange(bunkerTargetSounds);
                soundsPool.AddRange(observatoryTargetSounds);
                soundsPool.AddRange(warehouseTargetSounds);
            }
            else if (locationRng == 2)
            {
                soundsPool.AddRange(bunkerTargetSounds);
                soundsPool.AddRange(lighthouseTargetSounds);
                soundsPool.AddRange(warehouseTargetSounds);
            }
            else
            {
                soundsPool.AddRange(bunkerTargetSounds);
                soundsPool.AddRange(lighthouseTargetSounds);
                soundsPool.AddRange(observatoryTargetSounds);
            }

            audioSource.PlayOneShot(soundsPool.RandomItem());
            yield return new WaitForSeconds(waitToCorrectDuration);

            if (locationRng == 0)
                audioSource.PlayOneShot(correctBunkerSound);
            else if (locationRng == 1)
                audioSource.PlayOneShot(correctLighthouseSound);
            else if (locationRng == 2)
                audioSource.PlayOneShot(correctObservatorySound);
            else
                audioSource.PlayOneShot(correctWarehouseSound);
        }

        private void CatchPlayer()
        {
            isCatching = true;
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
                isleRoom.DeactivateIncorrectDoor();
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
