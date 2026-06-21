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
        [SerializeField] private float interaction_Distance = 5f;
        [SerializeField] private CluesController clues_Controller;

        private Transform camera_Transform;
        private HandInventory hand_Inventory;
        private MemoriesClass memories;
        private GameObject focused_Game_Object;
        private readonly List<Clue> active_Clues = new List<Clue>();

        [SerializeField] private Sprite interact_Clue;
        [SerializeField] private Sprite pick_Clue;
        [SerializeField] private Sprite memory_Clue;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference interactInstantAction;
        [SerializeField] private InputActionReference pickAction;
        [SerializeField] private InputActionReference rememberAction;

        private void OnEnable()
        {
            interactInstantAction.action.Enable();
            pickAction.action.Enable();
            rememberAction.action.Enable();
        }

        private void OnDisable()
        {
            interactInstantAction.action.Disable();
            pickAction.action.Disable();
            rememberAction.action.Disable();
        }

        private void Start()
        {
            camera_Transform = GetComponentInChildren<Camera>().transform;
            hand_Inventory = GetComponent<HandInventory>();
            memories = GetComponent<MemoriesClass>();
            if (camera_Transform == null)
            {
                Debug.LogError("Camera transform not found on player object or its children. Make sure there is a child object with a Camera component.");
                enabled = false;
            }

            if (clues_Controller == null)
            {
                Debug.LogError("Clues Controller is not set in the inspector. Please assign it.");
                enabled = false;
            }
        }

        private void Update()
        {
            Ray ray = new Ray(camera_Transform.position, camera_Transform.forward);
            GameObject currentFocusedObject = null;

            if (Physics.Raycast(ray, out RaycastHit hit, interaction_Distance))
            {
                currentFocusedObject = hit.collider.gameObject;

                // Handle IInteractive
                if (hit.collider.TryGetComponent(out IInteractive interactiveObject))
                {
                    if (interactInstantAction.action.WasPressedThisFrame())
                    {
                        interactiveObject.Interact();
                    }
                }

                // Handle IPicked
                if (hit.collider.TryGetComponent(out IPicked pickedObject))
                {
                    if (pickAction.action.WasPressedThisFrame())
                    {
                        if (hand_Inventory.TryEquip(pickedObject.Item))
                        {
                            pickedObject.Pick();
                        }
                    }
                }

                // Handle IMemoryable
                if (hit.collider.TryGetComponent(out IMemoryable memoryabledObject))
                {
                    if (rememberAction.action.WasPressedThisFrame())
                    {
                        memories.AddMemory(memoryabledObject.Memory);
                        memoryabledObject.Save();
                    }
                }
            }

            if (currentFocusedObject != focused_Game_Object)
            {
                if (focused_Game_Object != null && focused_Game_Object.TryGetComponent<IFocusable>(out IFocusable oldFocusable))
                    oldFocusable.OnEndFocus();

                focused_Game_Object = currentFocusedObject;

                if (focused_Game_Object != null && focused_Game_Object.TryGetComponent<IFocusable>(out IFocusable newFocusable))
                    newFocusable.OnBeginFocus();

                foreach (var clue in active_Clues)
                {
                    clues_Controller.Remove(clue);
                }
                active_Clues.Clear();

                if (focused_Game_Object != null)
                {
                    if (focused_Game_Object.TryGetComponent<IInteractive>(out IInteractive interactiveObject))
                    {
                        active_Clues.Add(clues_Controller.Create(interactiveObject.Name, interact_Clue));
                    }
                    if (focused_Game_Object.TryGetComponent<IPicked>(out _))
                    {
                        active_Clues.Add(clues_Controller.Create("Взять", pick_Clue));
                    }
                    if (focused_Game_Object.TryGetComponent<IMemoryable>(out _))
                    {
                        active_Clues.Add(clues_Controller.Create("Remember", memory_Clue));
                    }
                }
            }
            else
            {
                if (focused_Game_Object != null && focused_Game_Object.TryGetComponent<IFocusable>(out IFocusable focusableObject))
                    focusableObject.OnStartFocus();
            }
        }
    }
}
