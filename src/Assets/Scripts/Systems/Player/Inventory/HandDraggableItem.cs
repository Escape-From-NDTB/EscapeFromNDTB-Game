using NDTB.Data;
using NDTB.UI.Helpers;
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

        private HandInventory _handInventory;

        private SO_ItemData _currentItemData;
        private HandSlot _currentSlot;
        private Image _slotImage;

        private GameObject _draggedItemGo;
        private RectTransform _draggedItemRectTransform;
        private CanvasGroup _draggedItemCanvasGroup;

        private Image _indicatorOnDraggedItem;
        private RectTransform _indicatorRectTransformOnDraggedItem;
        private bool _isRotated = false;
        private bool _isInitialized = false;

        // --- Abstract property implementations ---
        protected override Image Indicator => _indicatorOnDraggedItem;
        protected override RectTransform DraggableRectTransform => _draggedItemRectTransform;
        protected override RectTransform IndicatorRectTransform => _indicatorRectTransformOnDraggedItem;
        protected override RectTransform GridContainer => _inventoryController.GridContainer;
        protected override int ItemWidth => _isRotated ? _currentItemData.Height : _currentItemData.Width;
        protected override int ItemHeight => _isRotated ? _currentItemData.Width : _currentItemData.Height;
        protected override InventoryItem ItemToIgnore => null;
        protected override void Rotate()
        {
            _isRotated = !_isRotated;
        }

        private void Start()
        {
            Initialize();
        }

        private bool Initialize()
        {
            if (_isInitialized) return true;

            _slotImage = GetComponent<Image>();
            _inventoryController = FindAnyObjectByType<InventoryController>();
            if (_inventoryController == null)
            {
                Debug.LogWarning("InventoryController not found in scene. Will try again on drag.");
                return false;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _handInventory = player.GetComponent<HandInventory>();
                _inventory = player.GetComponent<Inventory>();
            }
            else
            {
                Debug.LogError("Player object with tag 'Player' not found in scene.");
                return false;
            }

            if (IsPocket)
            {
                _currentSlot = (this.Hand == Hand.Left) ? _handInventory.LeftPocketSlot : _handInventory.RightPocketSlot;
            }
            else
            {
                _currentSlot = (this.Hand == Hand.Left) ? _handInventory.LeftHandSlot : _handInventory.RightHandSlot;
            }

            if (_currentSlot == null)
            {
                Debug.LogError("Current slot is null. Hand/Pocket slots might not be initialized in HandInventory.");
                return false;
            }

            _isInitialized = true;
            return true;
        }

        public override void DropItem()
        {
            _inventory.DropItemFromHand(this.Hand, IsPocket);
            _slotImage.enabled = true;
            Destroy(_draggedItemGo);
            _isDragged = false;
            _draggedItemGo = null;
            _draggedItemRectTransform = null;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isInitialized)
            {
                if (!Initialize())
                {
                    Debug.LogError("Failed to initialize HandDraggableItem on drag.");
                    return;
                }
            }

            _currentItemData = _currentSlot.CurrentItem;

            if (_currentItemData == null) return;

            if (_inventoryController.InventoryItemPrefab == null)
            {
                Debug.LogError("InventoryItemPrefab is not set in the InventoryController. Please assign it in the Inspector.");
                return;
            }

            _draggedItemGo = Instantiate(_inventoryController.InventoryItemPrefab, _inventoryController.DragAndDropContainer);
            _draggedItemRectTransform = _draggedItemGo.GetComponent<RectTransform>();

            DraggableItem originalDraggable = _draggedItemGo.GetComponent<DraggableItem>();
            if (originalDraggable != null)
            {
                originalDraggable.enabled = false;
                _indicatorOnDraggedItem = originalDraggable.IndicatorPublic;
            }

            Image draggedImage = _draggedItemGo.GetComponent<Image>();
            if (draggedImage != null)
            {
                draggedImage.sprite = _currentItemData.InventoryTexture;
            }

            _draggedItemCanvasGroup = _draggedItemGo.GetComponent<CanvasGroup>();
            _draggedItemCanvasGroup.blocksRaycasts = false;
            _draggedItemCanvasGroup.alpha = 0.7f;

            _draggedItemRectTransform.sizeDelta = new Vector2(_currentItemData.Width * _inventoryController.CellSize, _currentItemData.Height * _inventoryController.CellSize);

            if (_indicatorOnDraggedItem != null)
            {
                _indicatorRectTransformOnDraggedItem = _indicatorOnDraggedItem.GetComponent<RectTransform>();
                _indicatorRectTransformOnDraggedItem.sizeDelta = _draggedItemRectTransform.sizeDelta;
                _indicatorOnDraggedItem.color = _defaultColor;
            }

            _slotImage.enabled = false;
            _isRotated = false;
            _isDragged = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_draggedItemGo == null) { _slotImage.enabled = true; return; }

            _isDragged = false;

            RectTransform draggedRT = _draggedItemGo.GetComponent<RectTransform>();
            bool placedInSlot = false;

            var targets = new[] {
                new { slot = _handInventory.RightHandSlot, rt = _handInventory.RightHandInventory.GetComponent<RectTransform>(), type = "Hand", hand = Hand.Right },
                new { slot = _handInventory.LeftHandSlot, rt = _handInventory.LeftHandInventory.GetComponent<RectTransform>(), type = "Hand", hand = Hand.Left },
                new { slot = _handInventory.RightPocketSlot, rt = _handInventory.RightPocket.GetComponent<RectTransform>(), type = "Pocket", hand = Hand.Right },
                new { slot = _handInventory.LeftPocketSlot, rt = _handInventory.LeftPocket.GetComponent<RectTransform>(), type = "Pocket", hand = Hand.Left }
            };

            foreach (var target in targets)
            {
                if (_currentSlot != target.slot && target.slot.IsEmpty() && UIHelper.IsPartialOverlap(draggedRT, target.rt, _inventoryController.CellSize))
                {
                    SO_ItemData itemToMove = IsPocket ? _handInventory.UnequipFromPocket(this.Hand) : _handInventory.UnequipFromHand(this.Hand);

                    if (itemToMove != null)
                    {
                        if (target.type == "Hand") _handInventory.TryEquipToHand(itemToMove, target.hand);
                        else _handInventory.TryEquipToPocket(itemToMove, target.hand);

                        placedInSlot = true;
                        break;
                    }
                }
            }

            if (!placedInSlot)
            {
                var pos = GetXY(eventData);
                InventoryItem newItem = new InventoryItem(_currentItemData, pos.x, pos.y) { IsRotated = this._isRotated };

                if (_inventory.AddItem(newItem))
                {
                    if (IsPocket) _handInventory.UnequipFromPocket(this.Hand);
                    else _handInventory.UnequipFromHand(this.Hand);
                }
            }

            _slotImage.enabled = true;
            Destroy(_draggedItemGo);
            _draggedItemGo = null;
        }
    }
}
