using UnityEngine;
using UnityEngine.InputSystem;
namespace NDTB.Systems.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {

        [Header("Speeds")]
        [SerializeField] private readonly float _walkSpeed = 5;

        [SerializeField] private readonly float _runSpeed = 8;

        [SerializeField] private readonly float _stealSpeed = 3;

        [Header("------")]
        [SerializeField] private readonly float _jumpForce = 5;

        [SerializeField] private readonly float _gravity = -9.81f;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private InputSystem_Actions _input;

        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool JumpedThisFrame { get; private set; }

        public static bool ControlsInverted = false;

        private void Awake()
        {
            _input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        private void OnDestroy()
        {
            _input.Dispose();
        }

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            JumpedThisFrame = false;

            IsRunning = _input.Player.Sprint.IsPressed();
            IsCrouching = _input.Player.Crouch.IsPressed();

            float currentSpeed = _walkSpeed;
            if (IsRunning)
                currentSpeed = _runSpeed;
            else if (IsCrouching)
                currentSpeed = _stealSpeed;

            Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            if (ControlsInverted)
            {
                horizontal *= -1;
                vertical *= -1;
            }

            Vector3 move = transform.right * horizontal + transform.forward * vertical;

            Vector3 horizontalVelocity = move * currentSpeed;

            if (_characterController.isGrounded)
            {
                _velocity.y = -0.1f;
                if (_input.Player.Jump.IsPressed())
                {
                    _velocity.y = _jumpForce;
                    JumpedThisFrame = true;
                }
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }

            Vector3 finalVelocity = new Vector3(horizontalVelocity.x, _velocity.y, horizontalVelocity.z);
            _characterController.Move(finalVelocity * Time.deltaTime);

            if (IsCrouching)
                _characterController.height = 1f;
            else
                _characterController.height = 2f;
        }
    }
}
