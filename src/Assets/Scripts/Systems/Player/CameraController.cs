using UnityEngine;
using UnityEngine.InputSystem;

namespace NDTB.Systems.Player
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField][Range(1, 20)] private float rotate_Speed = 10;
        private Transform camera_Transform;
        private InputSystem_Actions input;

        private bool is_Active = false;

        private float rotation_X = 0f;

        private void Awake()
        {
            input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();
        }

        private void OnDestroy()
        {
            input.Dispose();
        }

        private void Start()
        {
            camera_Transform = GetComponentInChildren<Camera>().transform;
            Activate();
        }

        private void Update()
        {
            if (!is_Active) return;

            Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();
            float mouseX = lookInput.x * rotate_Speed * Time.deltaTime;
            float mouseY = lookInput.y * rotate_Speed * Time.deltaTime;

            transform.Rotate(Vector3.up * mouseX);

            rotation_X -= mouseY;
            rotation_X = Mathf.Clamp(rotation_X, -90f, 90f);
            camera_Transform.localRotation = Quaternion.Euler(rotation_X, 0f, 0f);
        }

        public void Activate()
        {
            is_Active = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Deactivate()
        {
            is_Active = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
