using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        private Inventory inventory;

        [Tooltip("The prefab for the visual item in the inventory.")]
        [SerializeField] private GameObject inventory_Item_Prefab;
        public GameObject InventoryItemPrefab => inventory_Item_Prefab;

        [Tooltip("The parent transform for the grid items.")]
        [SerializeField] private RectTransform grid_Container;
        public RectTransform GridContainer => grid_Container;

        [Tooltip("The parent transform for dragged items to ensure they stay on the canvas.")]
        [SerializeField] private Transform drag_And_Drop_Container;
        public Transform DragAndDropContainer => drag_And_Drop_Container;

        [Tooltip("The size of a single grid cell in pixels.")]
        [SerializeField] private float cell_Size = 100f;
        public float CellSize => cell_Size;

        // Maps the data item to its UI game object representation.
        private Dictionary<InventoryItem, GameObject> item_To_Game_Object_Map = new Dictionary<InventoryItem, GameObject>();

        private void Awake()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                inventory = player.GetComponent<Inventory>();
            }
        }

        private void OnEnable()
        {
            if (inventory == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    inventory = player.GetComponent<Inventory>();
                }
            }

            if (inventory == null)
            {
                Debug.LogError("Inventory component could not be found on a 'Player' tagged object. Disabling InventoryController.");
                this.enabled = false;
                return;
            }

            inventory.OnItemAdded += AddItemUI;
            inventory.OnItemRemoved += RemoveItemUI;

            RefreshAllItems();
        }

        private void OnDisable()
        {
            if (inventory == null) return;

            inventory.OnItemAdded -= AddItemUI;
            inventory.OnItemRemoved -= RemoveItemUI;

            ClearAllItemsUI();
        }

        private void AddItemUI(InventoryItem item)
        {
            if (inventory_Item_Prefab == null || grid_Container == null)
            {
                Debug.LogError("InventoryItemPrefab or GridContainer is not assigned.");
                return;
            }

            GameObject itemGO = Instantiate(inventory_Item_Prefab, grid_Container);
            item_To_Game_Object_Map[item] = itemGO;

            DraggableItem draggable = itemGO.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                draggable.Setup(inventory, this, item);
            }

            RectTransform rectTransform = itemGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            rectTransform.anchoredPosition = new Vector2(item.X * cell_Size, -item.Y * cell_Size);
            rectTransform.sizeDelta = new Vector2(item.GetWidth() * cell_Size, item.GetHeight() * cell_Size);
            draggable.IndicatorPublic.GetComponent<RectTransform>().sizeDelta = new Vector2(item.GetWidth() * cell_Size, item.GetHeight() * cell_Size);

            Image itemIcon = itemGO.GetComponentInChildren<Image>();
            if (itemIcon != null && item.ItemData.InventoryTexture != null)
            {
                itemIcon.sprite = item.ItemData.InventoryTexture;
            }
        }

        private void RemoveItemUI(InventoryItem item)
        {
            if (item_To_Game_Object_Map.ContainsKey(item))
            {
                Destroy(item_To_Game_Object_Map[item]);
                item_To_Game_Object_Map.Remove(item);
            }
        }

        private void RefreshAllItems()
        {
            if (inventory == null)
            {
                Debug.LogError("Inventory is unexpectedly null in RefreshAllItems despite previous checks.");
                return;
            }
            ClearAllItemsUI();
            foreach (var item in inventory.GetItems())
            {
                AddItemUI(item);
            }
        }

        private void ClearAllItemsUI()
        {
            foreach (var itemGO in item_To_Game_Object_Map.Values)
            {
                Destroy(itemGO);
            }
            item_To_Game_Object_Map.Clear();
        }
    }
}
