using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Inventory
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Image))]
    public class DraggableItem : AbstractDraggableItem
    {
        public InventoryItem InventoryItemRef { get; private set; }

        private RectTransform rect_Transform;
        private CanvasGroup canvas_Group;
        private Transform original_Parent;

        [Header("Indicator")]
        [SerializeField] private Image indicator;
        public Image IndicatorPublic => indicator;
        private RectTransform indicator_Rect_Transform;

        // --- Abstract property implementations ---
        protected override Image Indicator => indicator;
        protected override RectTransform DraggableRectTransform => rect_Transform;
        protected override RectTransform IndicatorRectTransform => indicator_Rect_Transform;
        protected override RectTransform GridContainer => (RectTransform)original_Parent;
        protected override int ItemWidth => InventoryItemRef.GetWidth();
        protected override int ItemHeight => InventoryItemRef.GetHeight();
        protected override InventoryItem ItemToIgnore => InventoryItemRef;
        protected override void Rotate()
        {
            InventoryItemRef.IsRotated = !InventoryItemRef.IsRotated;
        }

        public void Setup(Inventory inv, InventoryController controller, InventoryItem item)
        {
            Inventory = inv;
            Inventory_Controller = controller;
            InventoryItemRef = item;
            GetComponent<Image>().sprite = item.ItemData.Icon;
            indicator.color = Default_Color;
        }

        private void Awake()
        {
            rect_Transform = GetComponent<RectTransform>();
            canvas_Group = GetComponent<CanvasGroup>();
            indicator_Rect_Transform = indicator.GetComponent<RectTransform>();
        }

        public override void DropItem()
        {
            Inventory.DropItemFromGrid(InventoryItemRef);
            Destroy(gameObject);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (InventoryItemRef == null) return;

            original_Parent = rect_Transform.parent;

            // Visually lift the item
            rect_Transform.SetParent(Inventory_Controller.DragAndDropContainer);
            canvas_Group.blocksRaycasts = false;
            canvas_Group.alpha = 0.7f;

            Is_Dragged = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (InventoryItemRef == null) return;
            // Restore visual state
            canvas_Group.blocksRaycasts = true;
            canvas_Group.alpha = 1f;

            indicator.color = Default_Color;

            Is_Dragged = false;

            if (Inventory.IsPlacementToSlot(rect_Transform))
            {
                if (Inventory.EquipFromInventoryToHand(InventoryItemRef, rect_Transform))
                {
                    return;
                }
            }

            var pos = GetXY(eventData);

            Inventory.MoveItem(InventoryItemRef, pos.x, pos.y);

            rect_Transform.SetParent(original_Parent);
            rect_Transform.anchoredPosition = new Vector2(InventoryItemRef.X * Inventory_Controller.CellSize, -InventoryItemRef.Y * Inventory_Controller.CellSize);
        }
    }
}
