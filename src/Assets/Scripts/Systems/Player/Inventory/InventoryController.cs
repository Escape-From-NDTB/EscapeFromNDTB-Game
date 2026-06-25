using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace NDTB.Systems.Player.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        private Inventory _inventory;

        [Tooltip("The prefab for the visual item in the inventory.")]
        [SerializeField] private GameObject _inventoryItemPrefab;
        public GameObject InventoryItemPrefab => _inventoryItemPrefab;

        [Tooltip("The parent transform for the grid items.")]
        [SerializeField] private RectTransform _gridContainer;
        public RectTransform GridContainer => _gridContainer;

        [Tooltip("The parent transform for dragged items to ensure they stay on the canvas.")]
        [SerializeField] private Transform _dragAndDropContainer;
        public Transform DragAndDropContainer => _dragAndDropContainer;

        [Tooltip("The size of a single grid cell in pixels.")]
        [SerializeField] private readonly float _cellSize = 100f;
        public float CellSize => _cellSize;

        // Maps the data item to its UI game object representation.
        private readonly Dictionary<InventoryItem, GameObject> _itemToGameObjectMap = new Dictionary<InventoryItem, GameObject>();

        private void Awake()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _inventory = player.GetComponent<Inventory>();
            }
        }

        private void OnEnable()
        {
            if (_inventory == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _inventory = player.GetComponent<Inventory>();
                }
            }

            if (_inventory == null)
            {
                Debug.LogError("Inventory component could not be found on a 'Player' tagged object. Disabling InventoryController.");
                this.enabled = false;
                return;
            }

            _inventory.OnItemAdded += AddItemUI;
            _inventory.OnItemRemoved += RemoveItemUI;

            RefreshAllItems();
        }

        private void OnDisable()
        {
            if (_inventory == null) return;

            _inventory.OnItemAdded -= AddItemUI;
            _inventory.OnItemRemoved -= RemoveItemUI;

            ClearAllItemsUI();
        }

        private void AddItemUI(InventoryItem item)
        {
            if (_inventoryItemPrefab == null || _gridContainer == null)
            {
                Debug.LogError("InventoryItemPrefab or GridContainer is not assigned.");
                return;
            }

            GameObject itemGO = Instantiate(_inventoryItemPrefab, _gridContainer);
            _itemToGameObjectMap[item] = itemGO;

            DraggableItem draggable = itemGO.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                draggable.Setup(_inventory, this, item);
            }

            RectTransform rectTransform = itemGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            rectTransform.anchoredPosition = new Vector2(item.X * _cellSize, -item.Y * _cellSize);
            rectTransform.sizeDelta = new Vector2(item.GetWidth() * _cellSize, item.GetHeight() * _cellSize);
            draggable.IndicatorPublic.GetComponent<RectTransform>().sizeDelta = new Vector2(item.GetWidth() * _cellSize, item.GetHeight() * _cellSize);

            Image itemIcon = itemGO.GetComponentInChildren<Image>();
            if (itemIcon != null && item.ItemData.InventoryTexture != null)
            {
                itemIcon.sprite = item.ItemData.InventoryTexture;
            }
        }

        private void RemoveItemUI(InventoryItem item)
        {
            if (_itemToGameObjectMap.ContainsKey(item))
            {
                Destroy(_itemToGameObjectMap[item]);
                _itemToGameObjectMap.Remove(item);
            }
        }

        private void RefreshAllItems()
        {
            if (_inventory == null)
            {
                Debug.LogError("Inventory is unexpectedly null in RefreshAllItems despite previous checks.");
                return;
            }
            ClearAllItemsUI();
            foreach (var item in _inventory.GetItems())
            {
                AddItemUI(item);
            }
        }

        private void ClearAllItemsUI()
        {
            foreach (var itemGO in _itemToGameObjectMap.Values)
            {
                Destroy(itemGO);
            }
            _itemToGameObjectMap.Clear();
        }
    }
}
