using NDTB.UI.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace NDTB.Systems.Player.Inventory
{
    [RequireComponent(typeof(Image))]
    public abstract class AbstractDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // --- CONFIGURATION ---
        [Header("Indicator Colors")]
        [SerializeField] protected Color _defaultColor = new Color(0, 0, 0, 0);
        [SerializeField] protected Color _canColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] protected Color _cantColor = new Color(1, 0, 0, 0.5f);

        [Header("Input Actions")]
        [SerializeField] protected InputActionReference _rotateAction;

        [SerializeField] protected InputActionReference _dropAction;

        // --- PROTECTED STATE ---
        protected Inventory _inventory;
        protected InventoryController _inventoryController;
        protected bool _isDragged = false;

        // --- ABSTRACT MEMBERS ---
        protected abstract Image Indicator { get; }
        protected abstract RectTransform DraggableRectTransform { get; }
        protected abstract RectTransform IndicatorRectTransform { get; }
        protected abstract RectTransform GridContainer { get; }
        protected abstract int ItemWidth { get; }
        protected abstract int ItemHeight { get; }
        protected abstract InventoryItem ItemToIgnore { get; }
        protected abstract void Rotate();

        // --- INTERFACE IMPLEMENTATION ---
        public abstract void OnBeginDrag(PointerEventData eventData);
        public abstract void OnEndDrag(PointerEventData eventData);

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!_isDragged || DraggableRectTransform == null) return;

            DraggableRectTransform.position = eventData.position;

            if (Indicator == null) return;

            if (_inventory.IsPlacementToSlot(DraggableRectTransform))
            {
                Indicator.color = _canColor;
                return;
            }

            var pos = GetXY(eventData);
            Indicator.color = _inventory.IsPlacementPossible(pos.x, pos.y, ItemWidth, ItemHeight, ItemToIgnore) ? _canColor : _cantColor;
        }

        public abstract void DropItem();

        // --- VIRTUAL METHODS ---
        protected virtual void OnEnable()
        {
            _rotateAction.action.Enable();
            _dropAction.action.Enable();
        }

        protected virtual void OnDisable()
        {
            _rotateAction.action.Disable();
            _dropAction.action.Disable();
        }

        protected virtual void Update()
        {
            if (_isDragged && _rotateAction.action.WasPressedThisFrame())
            {
                Rotate();
                DraggableRectTransform.sizeDelta = new Vector2(ItemWidth * _inventoryController.CellSize, ItemHeight * _inventoryController.CellSize);
                if (IndicatorRectTransform != null)
                {
                    IndicatorRectTransform.sizeDelta = DraggableRectTransform.sizeDelta;
                }
            }
            if (_isDragged && _dropAction.action.WasPressedThisFrame())
            {
                DropItem();
            }
        }

        // --- HELPER METHODS ---
        protected (int x, int y) GetXY(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GridContainer,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);

            RectTransform parentRect = GridContainer;
            float deltaX = parentRect.rect.width / 2;
            float deltaY = parentRect.rect.height / 2;
            int newX = Mathf.FloorToInt((localPoint.x + deltaX) / _inventoryController.CellSize);
            int newY = Mathf.FloorToInt((-localPoint.y + deltaY) / _inventoryController.CellSize);

            return (newX, newY);
        }
    }
}
