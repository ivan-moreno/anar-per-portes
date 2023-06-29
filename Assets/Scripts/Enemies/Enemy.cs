using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace AnarPerPortes.Enemies
{
    public abstract class Enemy : MonoBehaviour
    {
        public bool IsRoblomanDisguise { get; private set; } = false;
        public EnemyTip Tip => tip;

        [SerializeField] protected SoundResource broskyTip;
        [SerializeField] private EnemyTip tip;
        [SerializeField] private PlayableDirector introDirector;
        [SerializeField] private GameObject roblomanDisguiseObject;

        protected AudioSource audioSource;
        protected Animator animator;
        protected Transform model;
        protected static bool isInIntro = false;
        protected bool isCatching = false;

        public virtual void Spawn()
        {
            EnemyManager.Singleton.MarkAsOperative(this);
        }

        public virtual void MarkAsRoblomanDisguise()
        {
            IsRoblomanDisguise = true;
            
            if (roblomanDisguiseObject != null)
                roblomanDisguiseObject.SetActive(true);
        }

        protected void CacheComponents()
        {
            audioSource = GetComponent<AudioSource>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
        }

        protected virtual void Despawn()
        {
            if (isCatching)
                return;

            EnemyManager.Singleton.UnmarkAsOperative(this);

            if (IsRoblomanDisguise)
                EnemyManager.Singleton.UnmarkAsOperative<RoblomanEnemy>();

            Destroy(gameObject);
        }

        protected virtual RoblomanEnemy RevealRoblomanDisguise()
        {
            return EnemyManager.Singleton.SpawnRoblomanAt(transform.position);
        }

        //TODO: Optimize this!
        protected IEnumerator IntroCinematicCoroutine()
        {
            isInIntro = true;
            PauseManager.Singleton.CanPause = false;
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.Camera.gameObject.SetActive(false);
            var directorUiCam = introDirector.transform.GetChild(0).GetChild(0).GetComponent<Camera>();
            FindObjectOfType<Canvas>().worldCamera = directorUiCam;
            PlayerController.Singleton.UiCamera.transform.localPosition = Vector3.zero;
            introDirector.gameObject.SetActive(true);
            yield return new WaitForSeconds((float)introDirector.playableAsset.duration);

            EnemyTipManager.Singleton.DisplayTip(Tip);
            yield return new WaitUntil(() => !EnemyTipManager.Singleton.IsDisplaying);

            introDirector.gameObject.SetActive(false);
            PlayerController.Singleton.Camera.gameObject.SetActive(true);
            FindObjectOfType<Canvas>().worldCamera = PlayerController.Singleton.UiCamera;
            PlayerController.Singleton.UnblockAll();
            PauseManager.Singleton.CanPause = true;
            isInIntro = false;
        }
    }
}
