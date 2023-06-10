using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Yusuf Enemy")]
    public class YusufEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Signs")]
        [SerializeField] private Material bunkerSignMaterial;
        [SerializeField] private Material lighthouseSignMaterial;
        [SerializeField] private Material observatorySignMaterial;
        [SerializeField] private Material warehouseSignMaterial;

        [Header("Sound")]
        [SerializeField] private SoundResource walkieTalkieAlertSound;
        [SerializeField] private SoundResource walkieTalkieStaticSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource[] bunkerTargetSounds;
        [SerializeField] private SoundResource[] lighthouseTargetSounds;
        [SerializeField] private SoundResource[] observatoryTargetSounds;
        [SerializeField] private SoundResource[] warehouseTargetSounds;

        private IsleRoom isleRoom;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            isleRoom = RoomManager.Singleton.LastLoadedRoom as IsleRoom;
            var targetPos = isleRoom.IncorrectDoor.transform.position + (isleRoom.IncorrectDoor.transform.forward * 4f);
            transform.position = targetPos;
            audioSource.PlayOneShot(walkieTalkieAlertSound);

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
            transform.LookAt(PlayerController.Singleton.transform.position);
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            IsOperative = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(1f);

            if (PlayerController.Singleton.EquippedItemIs("Roblobolita"))
            {
                isleRoom.CloseIncorrectDoor();
                PlayerController.Singleton.ConsumeEquippedItem();
                yield return new WaitForSeconds(0.5f);
                PlayerController.Singleton.ClearVisionTarget();
                PlayerController.Singleton.UnblockAll();
                Despawn();
                yield break;
            }

            CatchManager.Singleton.CatchPlayer("YUSUF ENDING", "Fuerzas Yusuf, Fuerzas Yusuf");
            audioSource.Play();
        }

        private void Despawn()
        {
            IsOperative = false;
            Destroy(gameObject);
        }
    }
}
