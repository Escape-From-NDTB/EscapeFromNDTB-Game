using NDTB.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Inventory
{
    public enum Hand
    {
        Left,
        Right
    }

    public class HandInventory : MonoBehaviour
    {
        public HandSlot LeftHandSlot;
        public HandSlot RightHandSlot;
        public HandSlot LeftPocketSlot;
        public HandSlot RightPocketSlot;

        private bool is_Left_Hand_Active = true;
        private bool is_Right_Hand_Active = true;

        private Inventory inventory;
        private InputSystem_Actions input;

        [Header("Hands")]
        [SerializeField] private GameObject left_In_Hand_Object;
        [SerializeField] private GameObject right_In_Hand_Object;
        [SerializeField] public Image LeftHandInventory;
        [SerializeField] public Image RightHandInventory;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference swapLeftPocketAction;
        [SerializeField] private InputActionReference swapRightPocketAction;
        [SerializeField] private InputActionReference swapHandsAction;
        [SerializeField] private InputActionReference dropAction;

        [Header("Pockets")]
        [SerializeField] public Image LeftPocket;
        [SerializeField] public Image RightPocket;
        [SerializeField] private Sprite default_Sprite;

        private void Awake()
        {
            input = new InputSystem_Actions();
            LeftHandSlot = new HandSlot();
            RightHandSlot = new HandSlot();
            LeftPocketSlot = new HandSlot();
            RightPocketSlot = new HandSlot();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                inventory = player.GetComponent<Inventory>();
            }
        }

        private void OnEnable()
        {
            input.Enable();
            swapLeftPocketAction.action.Enable();
            swapRightPocketAction.action.Enable();
            swapHandsAction.action.Enable();
            dropAction.action.Enable();
        }

        private void OnDisable()
        {
            input.Disable();
            swapLeftPocketAction.action.Disable();
            swapRightPocketAction.action.Disable();
            swapHandsAction.action.Disable();
            dropAction.action.Disable();
        }

        private void OnDestroy()
        {
            input.Dispose();
        }

        private void Start()
        {
            LeftPocket.sprite = default_Sprite;
            RightPocket.sprite = default_Sprite;
            LeftHandInventory.sprite = default_Sprite;
            RightHandInventory.sprite = default_Sprite;
        }

        private void Update()
        {
            if (swapLeftPocketAction.action.WasPressedThisFrame())
                SwapSlots(Hand.Left);

            if (swapRightPocketAction.action.WasPressedThisFrame())
                SwapSlots(Hand.Right);

            if (swapHandsAction.action.WasPressedThisFrame())
                SwapHands();

            if (dropAction.action.WasPressedThisFrame() && !inventory.IsOpen)
            {
                inventory.DropItemFromHand(input.Player.Crouch.IsPressed() ? Hand.Left : Hand.Right, false);
            }
        }

        private void OnItemEquipped(Hand hand, ItemData item)
        {
            if (item.IsTwoHanded)
            {
                if (hand == Hand.Left)
                {
                    is_Right_Hand_Active = false;
                }
                else
                {
                    is_Left_Hand_Active = false;
                }
            }
            GameObject targetObject = (hand == Hand.Left) ? left_In_Hand_Object : right_In_Hand_Object;
            Image targetHand = (hand == Hand.Left) ? LeftHandInventory : RightHandInventory;
            targetObject.GetComponent<MeshFilter>().mesh = item.Mesh;
            targetHand.sprite = item.Icon;
        }

        private void OnItemUnequipped(Hand hand, ItemData item)
        {
            if (item.IsTwoHanded)
            {
                is_Left_Hand_Active = true;
                is_Right_Hand_Active = true;
            }
            GameObject targetObject = (hand == Hand.Left) ? left_In_Hand_Object : right_In_Hand_Object;
            Image targetHand = (hand == Hand.Left) ? LeftHandInventory : RightHandInventory;
            targetObject.GetComponent<MeshFilter>().mesh = null;
            targetHand.sprite = default_Sprite;
        }

        public bool TryEquip(ItemData item)
        {
            if (is_Right_Hand_Active && RightHandSlot.IsEmpty())
                return TryEquipToHand(item, Hand.Right);
            if (is_Left_Hand_Active && LeftHandSlot.IsEmpty())
                return TryEquipToHand(item, Hand.Left);
            return false;
        }

        public bool TryEquipToHand(ItemData item, Hand hand)
        {
            HandSlot targetSlot = (hand == Hand.Left) ? LeftHandSlot : RightHandSlot;
            bool isHandActive = (hand == Hand.Left) ? is_Left_Hand_Active : is_Right_Hand_Active;

            if (isHandActive && targetSlot.IsEmpty())
            {
                if (item.IsTwoHanded)
                {
                    if (LeftHandSlot.IsEmpty() && RightHandSlot.IsEmpty() && is_Left_Hand_Active && is_Right_Hand_Active)
                    {
                        EquipToHand(targetSlot, item, hand);
                        return true;
                    }
                    else
                    {
                        Debug.Log("Cannot equip two-handed item, both hands must be free.");
                        return false;
                    }
                }
                else
                {
                    EquipToHand(targetSlot, item, hand);
                    return true;
                }
            }
            Debug.Log("Cannot equip item, hand is not empty or not active.");
            return false;
        }

        private void EquipToHand(HandSlot slot, ItemData item, Hand hand)
        {
            slot.Equip(item);
            OnItemEquipped(hand, item);
        }

        public bool TryEquipToPocket(ItemData item, Hand hand)
        {
            HandSlot targetSlot = (hand == Hand.Left) ? LeftPocketSlot : RightPocketSlot;
            if (!targetSlot.IsEmpty())
                return false;

            EquipToPocket(targetSlot, item, hand);
            return true;
        }

        private void EquipToPocket(HandSlot slot, ItemData item, Hand hand)
        {
            slot.Equip(item);
            OnItemPutToPocket(hand, item);
        }

        public void SwapSlots(Hand hand)
        {
            HandSlot handSlot = (hand == Hand.Left) ? LeftHandSlot : RightHandSlot;
            HandSlot pocketSlot = (hand == Hand.Left) ? LeftPocketSlot : RightPocketSlot;
            bool isHandActive = (hand == Hand.Left) ? is_Left_Hand_Active : is_Right_Hand_Active;

            if (!isHandActive)
            {
                Debug.Log("Cannot swap, hand slot is not active.");
                return;
            }

            ItemData unequippedHandItem = handSlot.Unequip();
            ItemData unequippedPocketItem = pocketSlot.Unequip();

            if (unequippedPocketItem != null && unequippedPocketItem.IsTwoHanded)
            {
                if (!LeftHandSlot.IsEmpty() || !RightHandSlot.IsEmpty())
                {
                    Debug.Log("Cannot equip two-handed item from pocket, both hands must be free.");
                    handSlot.Equip(unequippedHandItem);
                    pocketSlot.Equip(unequippedPocketItem);
                    return;
                }
            }

            if (unequippedHandItem != null)
            {
                OnItemUnequipped(hand, unequippedHandItem);
                pocketSlot.Equip(unequippedHandItem);
                OnItemPutToPocket(hand, unequippedHandItem);
            }

            if (unequippedPocketItem != null)
            {
                handSlot.Equip(unequippedPocketItem);
                if (unequippedHandItem == null)
                    OnItemGetFromPocket(hand);
                OnItemEquipped(hand, unequippedPocketItem);
            }
        }

        public void SwapHands()
        {
            if (is_Left_Hand_Active && is_Right_Hand_Active)
            {
                ItemData item_1 = UnequipFromHand(Hand.Left);
                ItemData item_2 = UnequipFromHand(Hand.Right);

                if (item_1 != null)
                    EquipToHand(RightHandSlot, item_1, Hand.Right);
                if (item_2 != null)
                    EquipToHand(LeftHandSlot, item_2, Hand.Left);
            }
        }

        public ItemData UnequipFromHand(Hand hand)
        {
            HandSlot handSlot = (hand == Hand.Left) ? LeftHandSlot : RightHandSlot;
            if (!handSlot.IsEmpty())
            {
                ItemData item = handSlot.Unequip();
                OnItemUnequipped(hand, item);
                return item;
            }
            return null;
        }

        public ItemData UnequipFromPocket(Hand hand)
        {
            HandSlot pocketSlot = (hand == Hand.Left) ? LeftPocketSlot : RightPocketSlot;
            if (!pocketSlot.IsEmpty())
            {
                ItemData item = pocketSlot.Unequip();
                OnItemGetFromPocket(hand);
                return item;
            }
            return null;
        }

        private void OnItemPutToPocket(Hand pocket, ItemData item)
        {
            Image pocketImage = (pocket == Hand.Left) ? LeftPocket : RightPocket;
            pocketImage.sprite = item.Icon;
        }

        private void OnItemGetFromPocket(Hand pocket)
        {
            Image pocketImage = (pocket == Hand.Left) ? LeftPocket : RightPocket;
            pocketImage.sprite = default_Sprite;
        }
    }
}
