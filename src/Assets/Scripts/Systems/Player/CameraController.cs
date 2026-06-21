using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField][Range(1, 20)] private float _rotateSpeed = 10;
    private Transform _cameraTransform;
    private InputSystem_Actions _input;

    private bool _isActive = false;

    private float _rotationX = 0f;

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

    void Start()
    {
        _cameraTransform = GetComponentInChildren<Camera>().transform;
        Activate();
    }

    void Update()
    {
        if (!_isActive) return;

        Vector2 lookInput = _input.Player.Look.ReadValue<Vector2>();
        float mouseX = lookInput.x * _rotateSpeed * Time.deltaTime;
        float mouseY = lookInput.y * _rotateSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
        _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
    }

    public void Activate()
    {
        _isActive = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Deactivate()
    {
        _isActive = false;
        Cursor.lockState = CursorLockMode.None;
    }
}
