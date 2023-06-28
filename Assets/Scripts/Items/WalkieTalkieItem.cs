using AnarPerPortes.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    public class WalkieTalkieItem : InventoryItem
    {
        [Header("Stats")]
        [SerializeField][Range(0f, 100f)] private float talkChance = 20f;
        [SerializeField][Min(0f)] private float warnCooldown = 90f;
        [SerializeField][Min(0f)] private float chatCooldown = 360f;

        [Header("Audio")]
        [SerializeField] private SoundResource[] chatAudios;
        [SerializeField] private SoundResource daviloteWarning;
        [SerializeField] private SoundResource pedroWarning;

        private float warnCooldownTimer = 0f;
        private float chatCooldownTimer = 0f;

        protected override void Start()
        {
            base.Start();

            chatCooldownTimer = chatCooldown;

            DaviloteEnemy.OnSpawn.AddListener(OnSpawnDavilote);
            PedroEnemy.OnSpawn.AddListener(OnSpawnPedro);
        }

        private void Update()
        {
            audioSource.volume = IsEquipped ? 1f : 0f;

            if (warnCooldownTimer > 0f)
                warnCooldownTimer -= Time.deltaTime;

            if (chatCooldownTimer > 0f && warnCooldownTimer <= 0)
                chatCooldownTimer -= Time.deltaTime;

            if (chatCooldownTimer <= 0f)
                RandomChat();
        }

        //TODO: Generic version of this method
        private void OnSpawnDavilote(DaviloteEnemy davilote)
        {
            if (warnCooldownTimer > 0f)
                return;

            if (Random.Range(0f, 100f) > talkChance)
                return;

            ItemManager.Singleton.CauseAlertOnItem(this);

            if (audioSource.volume > 0.1f)
                audioSource.PlayOneShot(daviloteWarning);
            else
                audioSource.PlayOneShot(daviloteWarning.AudioClip);

            warnCooldownTimer = warnCooldown;
        }

        private void OnSpawnPedro(PedroEnemy pedro)
        {
            if (warnCooldownTimer > 0f)
                return;

            if (Random.Range(0f, 100f) > talkChance)
                return;

            ItemManager.Singleton.CauseAlertOnItem(this);

            if (audioSource.volume > 0.1f)
                audioSource.PlayOneShot(pedroWarning);
            else
                audioSource.PlayOneShot(pedroWarning.AudioClip);

            warnCooldownTimer = warnCooldown;
        }

        private void RandomChat()
        {
            var audio = chatAudios.RandomItem();
            chatCooldownTimer = chatCooldown;
            warnCooldownTimer = audio.AudioClip.length;
            ItemManager.Singleton.CauseAlertOnItem(this);

            if (audioSource.volume > 0.1f)
                audioSource.PlayOneShot(audio);
            else
                audioSource.PlayOneShot(audio.AudioClip);
        }
    }
}
