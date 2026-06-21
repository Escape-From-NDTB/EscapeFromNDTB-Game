using System;
using System.Collections.Generic;
using NDTB.Data;
using NDTB.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Inventory
{
    public class Inventory : MonoBehaviour
    {
        public event Action<InventoryItem> OnItemAdded;
        public event Action<InventoryItem> OnItemRemoved;

        private HandInventory hand_Inventory;

        public bool IsOpen => inventory_UI.activeSelf;

        [Header("Grid Settings")]
        [SerializeField] private int width = 6;
        [SerializeField] private int height = 3;

        private InventoryItem[,] grid;
        private List<InventoryItem> items = new List<InventoryItem>();

        [Header("UI")]
        [SerializeField] private GameObject inventory_UI;

        private RectTransform left_Hand_Inventory_Rect;
        private RectTransform right_Hand_Inventory_Rect;
        private RectTransform left_Pocket_Rect;
        private RectTransform right_Pocket_Rect;

        [Header("Drop Settings")]
        private Transform player_Camera_Transform;
        [SerializeField] private float drop_Force = 5f;
        [SerializeField] private float drop_Offset = 1.5f;

        private void Awake()
        {
            grid = new InventoryItem[width, height];
            hand_Inventory = GetComponent<HandInventory>();
            player_Camera_Transform = GetComponentInChildren<Camera>().transform;

            left_Hand_Inventory_Rect = hand_Inventory.LeftHandInventory.GetComponent<RectTransform>();
            right_Hand_Inventory_Rect = hand_Inventory.RightHandInventory.GetComponent<RectTransform>();
            left_Pocket_Rect = hand_Inventory.LeftPocket.GetComponent<RectTransform>();
            right_Pocket_Rect = hand_Inventory.RightPocket.GetComponent<RectTransform>();
        }

        public void DropItemFromHand(Hand hand, bool isPocket)
        {
            var item = isPocket ? hand_Inventory.UnequipFromPocket(hand) : hand_Inventory.UnequipFromHand(hand);
            if (item == null)
                return;
            DropItem(item);
        }

        public void DropItemFromGrid(InventoryItem item)
        {
            if (item == null || item.ItemData.Prefab == null)
                return;

            DropItem(item.ItemData);
            RemoveItem(item);
        }

        private void DropItem(ItemData itemData)
        {
            Vector3 spawnPosition = player_Camera_Transform.position + player_Camera_Transform.forward * drop_Offset;
            GameObject droppedItemObj = Instantiate(itemData.Prefab, spawnPosition, Quaternion.identity);

            if (droppedItemObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(player_Camera_Transform.forward * drop_Force, ForceMode.Impulse);
            }
        }

        public List<InventoryItem> GetItems()
        {
            return items;
        }

        public bool IsPlacementPossible(InventoryItem item)
        {
            return IsPlacementPossible(item.X, item.Y, item.GetWidth(), item.GetHeight(), item);
        }

        public bool IsPlacementPossible(int startX, int startY, int itemWidth, int itemHeight, InventoryItem item)
        {
            if (startX < 0 || startY < 0 || startX + itemWidth > width || startY + itemHeight > height)
            {
                return false;
            }

            for (int i = 0; i < itemWidth; i++)
            {
                for (int j = 0; j < itemHeight; j++)
                {
                    if (grid[startX + i, startY + j] != null && grid[startX + i, startY + j] != item)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void PlaceItemOnGrid(InventoryItem item)
        {
            for (int i = 0; i < item.GetWidth(); i++)
            {
                for (int j = 0; j < item.GetHeight(); j++)
                {
                    grid[item.X + i, item.Y + j] = item;
                }
            }
        }

        private void RemoveItemFromGrid(InventoryItem item)
        {
            for (int i = 0; i < item.GetWidth(); i++)
            {
                for (int j = 0; j < item.GetHeight(); j++)
                {
                    int currentX = item.X + i;
                    int currentY = item.Y + j;
                    if (currentX < width && currentY < height && grid[currentX, currentY] == item)
                    {
                        grid[currentX, currentY] = null;
                    }
                }
            }
        }

        public bool AddItem(InventoryItem newItem)
        {
            if (!IsPlacementPossible(newItem))
            {
                return false;
            }

            items.Add(newItem);
            PlaceItemOnGrid(newItem);

            OnItemAdded?.Invoke(newItem);
            return true;
        }

        public void RemoveItem(InventoryItem itemToRemove)
        {
            if (itemToRemove == null || !items.Contains(itemToRemove))
            {
                return;
            }

            RemoveItemFromGrid(itemToRemove);
            items.Remove(itemToRemove);
            OnItemRemoved?.Invoke(itemToRemove);
        }

        public bool MoveItem(InventoryItem item, int newX, int newY)
        {
            RemoveItemFromGrid(item);

            int oldX = item.X;
            int oldY = item.Y;

            item.X = newX;
            item.Y = newY;

            if (IsPlacementPossible(item))
            {
                PlaceItemOnGrid(item);
                return true;
            }
            else
            {
                item.X = oldX;
                item.Y = oldY;
                PlaceItemOnGrid(item);
                return false;
            }
        }

        public bool IsPlacementToSlot(RectTransform item)
        {
            if (UIHelper.IsPartialOverlap(item, left_Hand_Inventory_Rect) && hand_Inventory.LeftHandSlot.IsEmpty())
                return true;
            if (UIHelper.IsPartialOverlap(item, right_Hand_Inventory_Rect) && hand_Inventory.RightHandSlot.IsEmpty())
                return true;
            if (UIHelper.IsPartialOverlap(item, left_Pocket_Rect) && hand_Inventory.LeftPocketSlot.IsEmpty())
                return true;
            if (UIHelper.IsPartialOverlap(item, right_Pocket_Rect) && hand_Inventory.RightPocketSlot.IsEmpty())
                return true;
            return false;
        }

        public bool EquipFromInventoryToHand(InventoryItem item, RectTransform rectTransformItem)
        {
            bool equipped = false;
            if (UIHelper.IsPartialOverlap(rectTransformItem, left_Hand_Inventory_Rect) && hand_Inventory.LeftHandSlot.IsEmpty())
                equipped = hand_Inventory.TryEquipToHand(item.ItemData, Hand.Left);
            else if (UIHelper.IsPartialOverlap(rectTransformItem, right_Hand_Inventory_Rect) && hand_Inventory.RightHandSlot.IsEmpty())
                equipped = hand_Inventory.TryEquipToHand(item.ItemData, Hand.Right);
            else if (UIHelper.IsPartialOverlap(rectTransformItem, left_Pocket_Rect) && hand_Inventory.LeftPocketSlot.IsEmpty())
                equipped = hand_Inventory.TryEquipToPocket(item.ItemData, Hand.Left);
            else if (UIHelper.IsPartialOverlap(rectTransformItem, right_Pocket_Rect) && hand_Inventory.RightPocketSlot.IsEmpty())
                equipped = hand_Inventory.TryEquipToPocket(item.ItemData, Hand.Right);

            if (equipped)
            {
                RemoveItem(item);
            }

            return equipped;
        }

        // FOR TEST
        public bool TryAddItem(ItemData itemData)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsPlacementPossible(x, y, itemData.Width, itemData.Height, null))
                    {
                        InventoryItem newItem = new InventoryItem(itemData, x, y);
                        AddItem(newItem);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
