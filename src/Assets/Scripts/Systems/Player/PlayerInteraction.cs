using System.Collections.Generic;
using NDTB.Core;
using NDTB.Systems.Player.Inventory;
using MemoriesClass = NDTB.Systems.Player.Memories.Memories;
using NDTB.UI.Clues;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace NDTB.Systems.Player
{
    public class PlayerInteraction : MonoBehaviour
    {

        [SerializeField] private float _interactionDistance = 5f;

        [SerializeField] private CluesController _cluesController;

        private Transform _cameraTransform;
        private HandInventory _handInventory;
        private MemoriesClass _memories;
        private GameObject _focusedGameObject;
        private readonly List<Clue> _activeClues = new List<Clue>();

        [SerializeField] private Sprite _interactClue;

        [SerializeField] private Sprite _pickClue;

        [SerializeField] private Sprite _memoryClue;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference _interactInstantAction;

        [SerializeField] private InputActionReference _pickAction;

        [SerializeField] private InputActionReference _rememberAction;

        private void OnEnable()
        {
            _interactInstantAction.action.Enable();
            _pickAction.action.Enable();
            _rememberAction.action.Enable();
        }

        private void OnDisable()
        {
            _interactInstantAction.action.Disable();
            _pickAction.action.Disable();
            _rememberAction.action.Disable();
        }

        private void Start()
        {
            _cameraTransform = GetComponentInChildren<Camera>().transform;
            _handInventory = GetComponent<HandInventory>();
            _memories = GetComponent<MemoriesClass>();
            if (_cameraTransform == null)
            {
                Debug.LogError("Camera transform not found on player object or its children. Make sure there is a child object with a Camera component.");
                enabled = false;
            }

            if (_cluesController == null)
            {
                Debug.LogError("Clues Controller is not set in the inspector. Please assign it.");
                enabled = false;
            }
        }

        private void Update()
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
            GameObject currentFocusedObject = null;

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance))
            {
                currentFocusedObject = hit.collider.gameObject;

                // Handle IInteractive
                if (hit.collider.TryGetComponent(out IInteractive interactiveObject))
                {
                    if (_interactInstantAction.action.WasPressedThisFrame())
                    {
                        interactiveObject.Interact();
                    }
                }

                // Handle IPicked
                if (hit.collider.TryGetComponent(out IPicked pickedObject))
                {
                    if (_pickAction.action.WasPressedThisFrame())
                    {
                        if (_handInventory.TryEquip(pickedObject.Item))
                        {
                            pickedObject.Pick();
                        }
                    }
                }

                // Handle IMemoryable
                if (hit.collider.TryGetComponent(out IMemoryable memoryabledObject))
                {
                    if (_rememberAction.action.WasPressedThisFrame())
                    {
                        _memories.AddMemory(memoryabledObject.Memory);
                        memoryabledObject.Save();
                    }
                }
            }

            if (currentFocusedObject != _focusedGameObject)
            {
                if (_focusedGameObject != null && _focusedGameObject.TryGetComponent<IFocusable>(out IFocusable oldFocusable))
                    oldFocusable.OnEndFocus();

                _focusedGameObject = currentFocusedObject;

                if (_focusedGameObject != null && _focusedGameObject.TryGetComponent<IFocusable>(out IFocusable newFocusable))
                    newFocusable.OnBeginFocus();

                foreach (var clue in _activeClues)
                {
                    _cluesController.Remove(clue);
                }
                _activeClues.Clear();

                if (_focusedGameObject != null)
                {
                    if (_focusedGameObject.TryGetComponent<IInteractive>(out IInteractive interactiveObject))
                    {
                        _activeClues.Add(_cluesController.Create(interactiveObject.Name, _interactClue));
                    }
                    if (_focusedGameObject.TryGetComponent<IPicked>(out _))
                    {
                        _activeClues.Add(_cluesController.Create("Взять", _pickClue));
                    }
                    if (_focusedGameObject.TryGetComponent<IMemoryable>(out _))
                    {
                        _activeClues.Add(_cluesController.Create("Remember", _memoryClue));
                    }
                }
            }
            else
            {
                if (_focusedGameObject != null && _focusedGameObject.TryGetComponent<IFocusable>(out IFocusable focusableObject))
                    focusableObject.OnStartFocus();
            }
        }
    }
}
