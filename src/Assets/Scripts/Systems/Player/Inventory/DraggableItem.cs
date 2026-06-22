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

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Transform _originalParent;

        [Header("Indicator")]
        [SerializeField] private Image _indicator;
        public Image IndicatorPublic => _indicator;
        private RectTransform _indicatorRectTransform;

        // --- Abstract property implementations ---
        protected override Image Indicator => _indicator;
        protected override RectTransform DraggableRectTransform => _rectTransform;
        protected override RectTransform IndicatorRectTransform => _indicatorRectTransform;
        protected override RectTransform GridContainer => (RectTransform)_originalParent;
        protected override int ItemWidth => InventoryItemRef.GetWidth();
        protected override int ItemHeight => InventoryItemRef.GetHeight();
        protected override InventoryItem ItemToIgnore => InventoryItemRef;
        protected override void Rotate()
        {
            InventoryItemRef.IsRotated = !InventoryItemRef.IsRotated;
        }

        public void Setup(Inventory inv, InventoryController controller, InventoryItem item)
        {
            _inventory = inv;
            _inventoryController = controller;
            InventoryItemRef = item;
            GetComponent<Image>().sprite = item.ItemData.Icon;
            _indicator.color = _defaultColor;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _indicatorRectTransform = _indicator.GetComponent<RectTransform>();
        }

        public override void DropItem()
        {
            _inventory.DropItemFromGrid(InventoryItemRef);
            Destroy(gameObject);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (InventoryItemRef == null) return;

            _originalParent = _rectTransform.parent;

            // Visually lift the item
            _rectTransform.SetParent(_inventoryController.DragAndDropContainer);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.7f;

            _isDragged = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (InventoryItemRef == null) return;
            // Restore visual state
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            _indicator.color = _defaultColor;

            _isDragged = false;

            if (_inventory.IsPlacementToSlot(_rectTransform))
            {
                if (_inventory.EquipFromInventoryToHand(InventoryItemRef, _rectTransform))
                {
                    return;
                }
            }

            var pos = GetXY(eventData);

            _inventory.MoveItem(InventoryItemRef, pos.x, pos.y);

            _rectTransform.SetParent(_originalParent);
            _rectTransform.anchoredPosition = new Vector2(InventoryItemRef.X * _inventoryController.CellSize, -InventoryItemRef.Y * _inventoryController.CellSize);
        }
    }
}
