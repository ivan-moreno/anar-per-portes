using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AnarPerPortes
{
    /// <summary>
    /// Logic class that reads Player input and acts accordingly.
    /// </summary>
    [AddComponentMenu("Anar per Portes/Player Controller")]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController characterController;
        private Animator visionAnimator;
        private new Camera camera;
        private Vector3 velocity;
        private Vector3 motion;
        private float vLook;
        private float walkSpeed = 8f;
        private const float vLookMaxAngle = 89.9f;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();

            // WARNING: Watch out when adding an Animator to the First Person model, as
            // GetComponentFromChildren also checks this transform for an Animator component.
            visionAnimator = GetComponentInChildren<Animator>();
            camera = visionAnimator.GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            UpdateRotation();
            UpdateMotion();

            Vector3 preMovePosition = transform.position;
            characterController.Move(motion);
            velocity = (transform.position - preMovePosition) / Time.deltaTime;
        }

        private void UpdateRotation()
        {
            var hLookInput = Input.GetAxisRaw("Mouse X");
            hLookInput *= Game.Settings.HMouseSensitivity;
            hLookInput *= Time.unscaledDeltaTime;

            var vLookInput = -Input.GetAxisRaw("Mouse Y");
            vLookInput *= Game.Settings.VMouseSensitivity;
            vLookInput *= Time.unscaledDeltaTime;

            // Limit vertical look angle to avoid flipping the camera.
            vLook = Mathf.Clamp(vLook + vLookInput, -vLookMaxAngle, vLookMaxAngle);

            transform.Rotate(0f, hLookInput, 0f);
            camera.transform.localEulerAngles = new(vLook, 0f, 0f);
        }

        private void UpdateMotion()
        {
            var moveXInput = Input.GetAxisRaw("Horizontal");
            var moveZInput = Input.GetAxisRaw("Vertical");

            motion = new(moveXInput, 0f, moveZInput);

            // Avoids faster speed when moving in diagonal.
            motion.Normalize();

            // Localizes motion to wherever the Player is looking.
            motion = transform.rotation * motion;

            // Applies the Walk Speed stat.
            motion *= walkSpeed;

            // Streamlines movement speed to be the same on any framerate.
            motion *= Time.deltaTime;
        }

        private void LateUpdate()
        {
            UpdateVisionAnimator();
        }

        private void UpdateVisionAnimator()
        {
            //TODO: This might provoke unintended offsets when disabling during gameplay.
            visionAnimator.enabled = Game.Settings.EnableVisionMotion;

            if (!visionAnimator.enabled)
                return;

            var hVelocity = new Vector3(velocity.x, 0f, velocity.z).sqrMagnitude;
            hVelocity /= walkSpeed * 8f;
            var animatorHVelocity = visionAnimator.GetFloat("HVelocity");
            var smoothHVelocity = Mathf.Lerp(animatorHVelocity, hVelocity, Time.deltaTime * 8f);
            visionAnimator.SetFloat("HVelocity", smoothHVelocity);
        }
    }
}
