using NDTB.UI;
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
        [SerializeField] protected Color Default_Color = new Color(0, 0, 0, 0);
        [SerializeField] protected Color Can_Color = new Color(0, 1, 0, 0.5f);
        [SerializeField] protected Color Cant_Color = new Color(1, 0, 0, 0.5f);

        [Header("Input Actions")]
        [SerializeField] protected InputActionReference rotateAction;
        [SerializeField] protected InputActionReference dropAction;

        // --- PROTECTED STATE ---
        protected Inventory Inventory;
        protected InventoryController Inventory_Controller;
        protected bool Is_Dragged = false;

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
            if (!Is_Dragged || DraggableRectTransform == null) return;

            DraggableRectTransform.position = eventData.position;

            if (Indicator == null) return;

            if (Inventory.IsPlacementToSlot(DraggableRectTransform))
            {
                Indicator.color = Can_Color;
                return;
            }

            var pos = GetXY(eventData);
            Indicator.color = Inventory.IsPlacementPossible(pos.x, pos.y, ItemWidth, ItemHeight, ItemToIgnore) ? Can_Color : Cant_Color;
        }

        public abstract void DropItem();

        // --- VIRTUAL METHODS ---
        protected virtual void OnEnable()
        {
            rotateAction.action.Enable();
            dropAction.action.Enable();
        }

        protected virtual void OnDisable()
        {
            rotateAction.action.Disable();
            dropAction.action.Disable();
        }

        protected virtual void Update()
        {
            if (Is_Dragged && rotateAction.action.WasPressedThisFrame())
            {
                Rotate();
                DraggableRectTransform.sizeDelta = new Vector2(ItemWidth * Inventory_Controller.CellSize, ItemHeight * Inventory_Controller.CellSize);
                if (IndicatorRectTransform != null)
                {
                    IndicatorRectTransform.sizeDelta = DraggableRectTransform.sizeDelta;
                }
            }
            if (Is_Dragged && dropAction.action.WasPressedThisFrame())
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
            int newX = Mathf.FloorToInt((localPoint.x + deltaX) / Inventory_Controller.CellSize);
            int newY = Mathf.FloorToInt((-localPoint.y + deltaY) / Inventory_Controller.CellSize);

            return (newX, newY);
        }
    }
}
