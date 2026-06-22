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

        private bool _isLeftHandActive = true;
        private bool _isRightHandActive = true;

        private Inventory _inventory;
        private InputSystem_Actions _input;

        [Header("Hands")]
        [SerializeField] private GameObject _leftInHandObject;

        [SerializeField] private GameObject _rightInHandObject;
        [SerializeField] public Image LeftHandInventory;
        [SerializeField] public Image RightHandInventory;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference _swapLeftPocketAction;

        [SerializeField] private InputActionReference _swapRightPocketAction;

        [SerializeField] private InputActionReference _swapHandsAction;

        [SerializeField] private InputActionReference _dropAction;

        [Header("Pockets")]
        [SerializeField] public Image LeftPocket;
        [SerializeField] public Image RightPocket;
        [SerializeField] private Sprite _defaultSprite;

        private void Awake()
        {
            _input = new InputSystem_Actions();
            LeftHandSlot = new HandSlot();
            RightHandSlot = new HandSlot();
            LeftPocketSlot = new HandSlot();
            RightPocketSlot = new HandSlot();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _inventory = player.GetComponent<Inventory>();
            }
        }

        private void OnEnable()
        {
            _input.Enable();
            _swapLeftPocketAction.action.Enable();
            _swapRightPocketAction.action.Enable();
            _swapHandsAction.action.Enable();
            _dropAction.action.Enable();
        }

        private void OnDisable()
        {
            _input.Disable();
            _swapLeftPocketAction.action.Disable();
            _swapRightPocketAction.action.Disable();
            _swapHandsAction.action.Disable();
            _dropAction.action.Disable();
        }

        private void OnDestroy()
        {
            _input.Dispose();
        }

        private void Start()
        {
            LeftPocket.sprite = _defaultSprite;
            RightPocket.sprite = _defaultSprite;
            LeftHandInventory.sprite = _defaultSprite;
            RightHandInventory.sprite = _defaultSprite;
        }

        private void Update()
        {
            if (_swapLeftPocketAction.action.WasPressedThisFrame())
                SwapSlots(Hand.Left);

            if (_swapRightPocketAction.action.WasPressedThisFrame())
                SwapSlots(Hand.Right);

            if (_swapHandsAction.action.WasPressedThisFrame())
                SwapHands();

            if (_dropAction.action.WasPressedThisFrame() && !_inventory.IsOpen)
            {
                _inventory.DropItemFromHand(_input.Player.Crouch.IsPressed() ? Hand.Left : Hand.Right, false);
            }
        }

        private void OnItemEquipped(Hand hand, SO_ItemData item)
        {
            if (item.IsTwoHanded)
            {
                if (hand == Hand.Left)
                {
                    _isRightHandActive = false;
                }
                else
                {
                    _isLeftHandActive = false;
                }
            }
            GameObject targetObject = (hand == Hand.Left) ? _leftInHandObject : _rightInHandObject;
            Image targetHand = (hand == Hand.Left) ? LeftHandInventory : RightHandInventory;
            targetObject.GetComponent<MeshFilter>().mesh = item.Mesh;
            targetHand.sprite = item.Icon;
        }

        private void OnItemUnequipped(Hand hand, SO_ItemData item)
        {
            if (item.IsTwoHanded)
            {
                _isLeftHandActive = true;
                _isRightHandActive = true;
            }
            GameObject targetObject = (hand == Hand.Left) ? _leftInHandObject : _rightInHandObject;
            Image targetHand = (hand == Hand.Left) ? LeftHandInventory : RightHandInventory;
            targetObject.GetComponent<MeshFilter>().mesh = null;
            targetHand.sprite = _defaultSprite;
        }

        public bool TryEquip(SO_ItemData item)
        {
            if (_isRightHandActive && RightHandSlot.IsEmpty())
                return TryEquipToHand(item, Hand.Right);
            if (_isLeftHandActive && LeftHandSlot.IsEmpty())
                return TryEquipToHand(item, Hand.Left);
            return false;
        }

        public bool TryEquipToHand(SO_ItemData item, Hand hand)
        {
            HandSlot targetSlot = (hand == Hand.Left) ? LeftHandSlot : RightHandSlot;
            bool isHandActive = (hand == Hand.Left) ? _isLeftHandActive : _isRightHandActive;

            if (isHandActive && targetSlot.IsEmpty())
            {
                if (item.IsTwoHanded)
                {
                    if (LeftHandSlot.IsEmpty() && RightHandSlot.IsEmpty() && _isLeftHandActive && _isRightHandActive)
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

        private void EquipToHand(HandSlot slot, SO_ItemData item, Hand hand)
        {
            slot.Equip(item);
            OnItemEquipped(hand, item);
        }

        public bool TryEquipToPocket(SO_ItemData item, Hand hand)
        {
            HandSlot targetSlot = (hand == Hand.Left) ? LeftPocketSlot : RightPocketSlot;
            if (!targetSlot.IsEmpty())
                return false;

            EquipToPocket(targetSlot, item, hand);
            return true;
        }

        private void EquipToPocket(HandSlot slot, SO_ItemData item, Hand hand)
        {
            slot.Equip(item);
            OnItemPutToPocket(hand, item);
        }

        public void SwapSlots(Hand hand)
        {
            HandSlot handSlot = (hand == Hand.Left) ? LeftHandSlot : RightHandSlot;
            HandSlot pocketSlot = (hand == Hand.Left) ? LeftPocketSlot : RightPocketSlot;
            bool isHandActive = (hand == Hand.Left) ? _isLeftHandActive : _isRightHandActive;

            if (!isHandActive)
            {
                Debug.Log("Cannot swap, hand slot is not active.");
                return;
            }

            SO_ItemData unequippedHandItem = handSlot.Unequip();
            SO_ItemData unequippedPocketItem = pocketSlot.Unequip();

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
            if (_isLeftHandActive && _isRightHandActive)
            {
                SO_ItemData item_1 = UnequipFromHand(Hand.Left);
                SO_ItemData item_2 = UnequipFromHand(Hand.Right);

                if (item_1 != null)
                    EquipToHand(RightHandSlot, item_1, Hand.Right);
                if (item_2 != null)
                    EquipToHand(LeftHandSlot, item_2, Hand.Left);
            }
        }

        public SO_ItemData UnequipFromHand(Hand hand)
        {
            HandSlot handSlot = (hand == Hand.Left) ? LeftHandSlot : RightHandSlot;
            if (!handSlot.IsEmpty())
            {
                SO_ItemData item = handSlot.Unequip();
                OnItemUnequipped(hand, item);
                return item;
            }
            return null;
        }

        public SO_ItemData UnequipFromPocket(Hand hand)
        {
            HandSlot pocketSlot = (hand == Hand.Left) ? LeftPocketSlot : RightPocketSlot;
            if (!pocketSlot.IsEmpty())
            {
                SO_ItemData item = pocketSlot.Unequip();
                OnItemGetFromPocket(hand);
                return item;
            }
            return null;
        }

        private void OnItemPutToPocket(Hand pocket, SO_ItemData item)
        {
            Image pocketImage = (pocket == Hand.Left) ? LeftPocket : RightPocket;
            pocketImage.sprite = item.Icon;
        }

        private void OnItemGetFromPocket(Hand pocket)
        {
            Image pocketImage = (pocket == Hand.Left) ? LeftPocket : RightPocket;
            pocketImage.sprite = _defaultSprite;
        }
    }
}
