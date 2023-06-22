using AnarPerPortes.Rooms;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Entities/Pom Pom Entity")]
    public class PomPomEntity : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField][Min(0f)] private float walkSpeed = 1f;
        [SerializeField][Min(0f)] private float runSpeed = 8f;
        [SerializeField][Min(0f)] private float acceleration = 8f;
        [SerializeField][Min(0f)] private float runDistance = 7f;
        [SerializeField][Min(0f)] private float waitDistance = 5f;
        [SerializeField][Min(0f)] private float smileDistance = 2f;

        [Header("Components")]
        [SerializeField] private Animator faceAnimator;

        CharacterController characterController;
        private Animator animator;
        private Transform model;
        private Vector3 moveVector;
        private Vector3 pushVector;
        private float currentSpeed;
        private float smileCooldown;
        private bool reachedTarget = false;

        private void Start()
        {
            transform.SetParent(null);
            characterController = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
            smileCooldown = Random.Range(9f, 15f);
        }

        private void LookAtDirection(Vector3 direction)
        {
            if (direction.magnitude < Mathf.Epsilon)
                return;

            direction.y = 0f;

            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(model.rotation, targetRotation, 90f * Time.deltaTime);
        }

        private void Update()
        {
            var distanceToPlayer = DistanceToPlayer(transform);

            reachedTarget = distanceToPlayer < waitDistance;

            smileCooldown -= Time.deltaTime;

            if (smileCooldown <= 0f)
                Smile();

            /*if (distanceToPlayer < smileDistance)
            {
                smileCooldown -= Time.deltaTime;

                if (smileCooldown <= 0f)
                    Smile();
            }*/

            var targetSpeed = distanceToPlayer > runDistance ? runSpeed : walkSpeed;

            if (!reachedTarget)
            {
                moveVector = transform.forward;
                var direction = Vector3.Normalize(PlayerPosition() - transform.position);
                LookAtDirection(direction);
            }
            else
                moveVector = Vector3.zero;

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            moveVector *= currentSpeed;
            moveVector.y = -24f;
            characterController.Move((moveVector + pushVector) * Time.deltaTime);
            animator.SetBool("IsWalking", !reachedTarget);
            animator.SetBool("IsRunning", distanceToPlayer > runDistance);
            faceAnimator.SetBool("IsRunning", distanceToPlayer > runDistance);

            if (transform.position.y < -256f)
                Teleport(PlayerPosition() - PlayerController.Singleton.transform.forward * 5f);
        }

        private void Smile()
        {
            smileCooldown = Random.Range(9f, 15f);
            faceAnimator.SetTrigger("Smile");
        }

        private void Teleport(Vector3 location)
        {
            characterController.enabled = false;
            transform.position = location;
            characterController.enabled = true;
        }
    }
}
