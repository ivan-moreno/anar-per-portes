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
        private new Camera camera;
        private Vector3 motion;
        private float vLook;
        private float walkSpeed = 8f;

        private const float vLookMaxAngle = 89.9f;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            camera = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            UpdateRotation();
            UpdateMotion();
            characterController.Move(motion);
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
    }
}
