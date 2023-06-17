using UnityEngine;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Danylopez Enemy")]
    [RequireComponent(typeof(AudioSource))]
    public class DanylopezEnemy : Enemy
    {
        public override void Spawn()
        {
            EnemyManager.Singleton.MarkAsOperative(this);
            audioSource = GetComponent<AudioSource>();

            PauseManager.Singleton.OnPauseChanged.AddListener(OnPauseChanged);
        }

        protected override void Despawn()
        {
            EnemyManager.Singleton.UnmarkAsOperative(this);
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }
    }
}
