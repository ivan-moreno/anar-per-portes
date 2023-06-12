using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    public class CatalanBirdEntity : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Interactable diceInteractable;
        [SerializeField] private Transform diceABone;
        [SerializeField] private Transform diceBBone;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float saluteDistance = 5f;

        [Header("Audio")]
        [SerializeField] private SoundResource curseSound;
        [SerializeField] private SoundResource[] saluteSounds;

        private Animator animator;
        private AudioSource audioSource;
        private bool hasSaluted = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();

            diceInteractable.OnInteracted.AddListener(OnDiceInteracted);
        }

        private void OnDiceInteracted()
        {
            StartCoroutine(nameof(OnDiceInteractedCoroutine));
        }

        private IEnumerator OnDiceInteractedCoroutine()
        {
            diceInteractable.gameObject.SetActive(false);
            animator.Play("Roll");

            yield return new WaitForSeconds(0.56f);

            var rng = Random.Range(1, 7);

            diceBBone.localEulerAngles = rng switch
            {
                1 => Vector3.zero,
                2 => new(-90f, 0f, 0f),
                3 => new(-180f, 0f, 0f),
                4 => new(90f, 0f, 0f),
                5 => new(0f, -90f, 0f),
                6 => new(0f, 90f, 0f),
                _ => Vector3.zero,

            };

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.97f);

            if (rng == 2)
            {
                animator.Play("GiveCurse");
                yield return new WaitForSeconds(2f);
                PlayerSound(curseSound);
                Destroy(gameObject);
            }
            else
            {
                animator.Play("GiveReward");
            }
        }

        private void FixedUpdate()
        {
            if (hasSaluted)
                return;

            var distance = Vector3.Distance(transform.position, PlayerPosition());

            if (distance < saluteDistance)
            {
                hasSaluted = true;
                animator.Play("Salute");
                audioSource.PlayOneShot(saluteSounds.RandomItem());
            }
        }
    }
}
