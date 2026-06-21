using NDTB.Data;
using NDTB.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Inventory
{
    [RequireComponent(typeof(Image))]
    public class HandDraggableItem : AbstractDraggableItem
    {
        public Hand Hand;
        public bool IsPocket;

        private HandInventory hand_Inventory;

        private ItemData current_Item_Data;
        private HandSlot current_Slot;
        private Image slot_Image;

        private GameObject dragged_Item_GO;
        private RectTransform dragged_Item_Rect_Transform;
        private CanvasGroup dragged_Item_Canvas_Group;

        private Image indicator_On_Dragged_Item;
        private RectTransform indicator_Rect_Transform_On_Dragged_Item;
        private bool is_Rotated = false;
        private bool is_Initialized = false;

        // --- Abstract property implementations ---
        protected override Image Indicator => indicator_On_Dragged_Item;
        protected override RectTransform DraggableRectTransform => dragged_Item_Rect_Transform;
        protected override RectTransform IndicatorRectTransform => indicator_Rect_Transform_On_Dragged_Item;
        protected override RectTransform GridContainer => Inventory_Controller.GridContainer;
        protected override int ItemWidth => is_Rotated ? current_Item_Data.Height : current_Item_Data.Width;
        protected override int ItemHeight => is_Rotated ? current_Item_Data.Width : current_Item_Data.Height;
        protected override InventoryItem ItemToIgnore => null;
        protected override void Rotate()
        {
            is_Rotated = !is_Rotated;
        }

        private void Start()
        {
            Initialize();
        }

        private bool Initialize()
        {
            if (is_Initialized) return true;

            slot_Image = GetComponent<Image>();
            Inventory_Controller = FindAnyObjectByType<InventoryController>();
            if (Inventory_Controller == null)
            {
                Debug.LogWarning("InventoryController not found in scene. Will try again on drag.");
                return false;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                hand_Inventory = player.GetComponent<HandInventory>();
                Inventory = player.GetComponent<Inventory>();
            }
            else
            {
                Debug.LogError("Player object with tag 'Player' not found in scene.");
                return false;
            }

            if (IsPocket)
            {
                current_Slot = (this.Hand == Hand.Left) ? hand_Inventory.LeftPocketSlot : hand_Inventory.RightPocketSlot;
            }
            else
            {
                current_Slot = (this.Hand == Hand.Left) ? hand_Inventory.LeftHandSlot : hand_Inventory.RightHandSlot;
            }

            if (current_Slot == null)
            {
                Debug.LogError("Current slot is null. Hand/Pocket slots might not be initialized in HandInventory.");
                return false;
            }

            is_Initialized = true;
            return true;
        }

        public override void DropItem()
        {
            Inventory.DropItemFromHand(this.Hand, IsPocket);
            slot_Image.enabled = true;
            Destroy(dragged_Item_GO);
            Is_Dragged = false;
            dragged_Item_GO = null;
            dragged_Item_Rect_Transform = null;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!is_Initialized)
            {
                if (!Initialize())
                {
                    Debug.LogError("Failed to initialize HandDraggableItem on drag.");
                    return;
                }
            }

            current_Item_Data = current_Slot.CurrentItem;

            if (current_Item_Data == null) return;

            if (Inventory_Controller.InventoryItemPrefab == null)
            {
                Debug.LogError("InventoryItemPrefab is not set in the InventoryController. Please assign it in the Inspector.");
                return;
            }

            dragged_Item_GO = Instantiate(Inventory_Controller.InventoryItemPrefab, Inventory_Controller.DragAndDropContainer);
            dragged_Item_Rect_Transform = dragged_Item_GO.GetComponent<RectTransform>();

            DraggableItem originalDraggable = dragged_Item_GO.GetComponent<DraggableItem>();
            if (originalDraggable != null)
            {
                originalDraggable.enabled = false;
                indicator_On_Dragged_Item = originalDraggable.IndicatorPublic;
            }

            Image draggedImage = dragged_Item_GO.GetComponent<Image>();
            if (draggedImage != null)
            {
                draggedImage.sprite = current_Item_Data.InventoryTexture;
            }

            dragged_Item_Canvas_Group = dragged_Item_GO.GetComponent<CanvasGroup>();
            dragged_Item_Canvas_Group.blocksRaycasts = false;
            dragged_Item_Canvas_Group.alpha = 0.7f;

            dragged_Item_Rect_Transform.sizeDelta = new Vector2(current_Item_Data.Width * Inventory_Controller.CellSize, current_Item_Data.Height * Inventory_Controller.CellSize);

            if (indicator_On_Dragged_Item != null)
            {
                indicator_Rect_Transform_On_Dragged_Item = indicator_On_Dragged_Item.GetComponent<RectTransform>();
                indicator_Rect_Transform_On_Dragged_Item.sizeDelta = dragged_Item_Rect_Transform.sizeDelta;
                indicator_On_Dragged_Item.color = Default_Color;
            }

            slot_Image.enabled = false;
            is_Rotated = false;
            Is_Dragged = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (dragged_Item_GO == null) { slot_Image.enabled = true; return; }

            Is_Dragged = false;

            RectTransform draggedRT = dragged_Item_GO.GetComponent<RectTransform>();
            bool placedInSlot = false;

            var targets = new[] {
                new { slot = hand_Inventory.RightHandSlot, rt = hand_Inventory.RightHandInventory.GetComponent<RectTransform>(), type = "Hand", hand = Hand.Right },
                new { slot = hand_Inventory.LeftHandSlot, rt = hand_Inventory.LeftHandInventory.GetComponent<RectTransform>(), type = "Hand", hand = Hand.Left },
                new { slot = hand_Inventory.RightPocketSlot, rt = hand_Inventory.RightPocket.GetComponent<RectTransform>(), type = "Pocket", hand = Hand.Right },
                new { slot = hand_Inventory.LeftPocketSlot, rt = hand_Inventory.LeftPocket.GetComponent<RectTransform>(), type = "Pocket", hand = Hand.Left }
            };

            foreach (var target in targets)
            {
                if (current_Slot != target.slot && target.slot.IsEmpty() && UIHelper.IsPartialOverlap(draggedRT, target.rt, Inventory_Controller.CellSize))
                {
                    ItemData itemToMove = IsPocket ? hand_Inventory.UnequipFromPocket(this.Hand) : hand_Inventory.UnequipFromHand(this.Hand);

                    if (itemToMove != null)
                    {
                        if (target.type == "Hand") hand_Inventory.TryEquipToHand(itemToMove, target.hand);
                        else hand_Inventory.TryEquipToPocket(itemToMove, target.hand);

                        placedInSlot = true;
                        break;
                    }
                }
            }

            if (!placedInSlot)
            {
                var pos = GetXY(eventData);
                InventoryItem newItem = new InventoryItem(current_Item_Data, pos.x, pos.y) { IsRotated = this.is_Rotated };

                if (Inventory.AddItem(newItem))
                {
                    if (IsPocket) hand_Inventory.UnequipFromPocket(this.Hand);
                    else hand_Inventory.UnequipFromHand(this.Hand);
                }
            }

            slot_Image.enabled = true;
            Destroy(dragged_Item_GO);
            dragged_Item_GO = null;
        }
    }
}
