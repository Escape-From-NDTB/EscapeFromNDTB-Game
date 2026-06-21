using UnityEngine;
using UnityEngine.InputSystem;

namespace NDTB.Systems.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Speeds")]
        [SerializeField] private float walk_Speed = 5;
        [SerializeField] private float run_Speed = 8;
        [SerializeField] private float steal_Speed = 3;

        [Header("------")]
        [SerializeField] private float jump_Force = 5;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController character_Controller;
        private Vector3 velocity;
        private InputSystem_Actions input;

        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool JumpedThisFrame { get; private set; }

        public static bool ControlsInverted = false;

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
            character_Controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            JumpedThisFrame = false;

            IsRunning = input.Player.Sprint.IsPressed();
            IsCrouching = input.Player.Crouch.IsPressed();

            float currentSpeed = walk_Speed;
            if (IsRunning)
                currentSpeed = run_Speed;
            else if (IsCrouching)
                currentSpeed = steal_Speed;

            Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            if (ControlsInverted)
            {
                horizontal *= -1;
                vertical *= -1;
            }

            Vector3 move = transform.right * horizontal + transform.forward * vertical;

            Vector3 horizontalVelocity = move * currentSpeed;

            if (character_Controller.isGrounded)
            {
                velocity.y = -0.1f;
                if (input.Player.Jump.IsPressed())
                {
                    velocity.y = jump_Force;
                    JumpedThisFrame = true;
                }
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }

            Vector3 finalVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
            character_Controller.Move(finalVelocity * Time.deltaTime);

            if (IsCrouching)
                character_Controller.height = 1f;
            else
                character_Controller.height = 2f;
        }
    }
}
