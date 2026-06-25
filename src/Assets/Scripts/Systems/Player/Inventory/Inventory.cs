using System;
using System.Collections.Generic;
using NDTB.Data;
using NDTB.UI.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Inventory
{
    public class Inventory : MonoBehaviour
    {
        public event Action<InventoryItem> OnItemAdded;
        public event Action<InventoryItem> OnItemRemoved;

        private HandInventory _handInventory;

        public bool IsOpen => _inventoryUi.activeSelf;

        [Header("Grid Settings")]
        [SerializeField] private readonly int _width = 6;
        [SerializeField] private readonly int _height = 3;

        private InventoryItem[,] _grid;
        private readonly List<InventoryItem> _items = new List<InventoryItem>();

        [Header("UI")]
        [SerializeField] private GameObject _inventoryUi;

        private RectTransform _leftHandInventoryRect;
        private RectTransform _rightHandInventoryRect;
        private RectTransform _leftPocketRect;
        private RectTransform _rightPocketRect;

        [Header("Drop Settings")]
        private Transform _playerCameraTransform;
        [SerializeField] private readonly float _dropForce = 5f;
        [SerializeField] private readonly float _dropOffset = 1.5f;

        private void Awake()
        {
            _grid = new InventoryItem[_width, _height];
            _handInventory = GetComponent<HandInventory>();
            _playerCameraTransform = GetComponentInChildren<Camera>().transform;

            _leftHandInventoryRect = _handInventory.LeftHandInventory.GetComponent<RectTransform>();
            _rightHandInventoryRect = _handInventory.RightHandInventory.GetComponent<RectTransform>();
            _leftPocketRect = _handInventory.LeftPocket.GetComponent<RectTransform>();
            _rightPocketRect = _handInventory.RightPocket.GetComponent<RectTransform>();
        }

        public void DropItemFromHand(Hand hand, bool isPocket)
        {
            var item = isPocket ? _handInventory.UnequipFromPocket(hand) : _handInventory.UnequipFromHand(hand);
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

        private void DropItem(SO_ItemData itemData)
        {
            Vector3 spawnPosition = _playerCameraTransform.position + _playerCameraTransform.forward * _dropOffset;
            GameObject droppedItemObj = Instantiate(itemData.Prefab, spawnPosition, Quaternion.identity);

            if (droppedItemObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(_playerCameraTransform.forward * _dropForce, ForceMode.Impulse);
            }
        }

        public List<InventoryItem> GetItems()
        {
            return _items;
        }

        public bool IsPlacementPossible(InventoryItem item)
        {
            return IsPlacementPossible(item.X, item.Y, item.GetWidth(), item.GetHeight(), item);
        }

        public bool IsPlacementPossible(int startX, int startY, int itemWidth, int itemHeight, InventoryItem item)
        {
            if (startX < 0 || startY < 0 || startX + itemWidth > _width || startY + itemHeight > _height)
            {
                return false;
            }

            for (int i = 0; i < itemWidth; i++)
            {
                for (int j = 0; j < itemHeight; j++)
                {
                    if (_grid[startX + i, startY + j] != null && _grid[startX + i, startY + j] != item)
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
                    _grid[item.X + i, item.Y + j] = item;
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
                    if (currentX < _width && currentY < _height && _grid[currentX, currentY] == item)
                    {
                        _grid[currentX, currentY] = null;
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

            _items.Add(newItem);
            PlaceItemOnGrid(newItem);

            OnItemAdded?.Invoke(newItem);
            return true;
        }

        public void RemoveItem(InventoryItem itemToRemove)
        {
            if (itemToRemove == null || !_items.Contains(itemToRemove))
            {
                return;
            }

            RemoveItemFromGrid(itemToRemove);
            _items.Remove(itemToRemove);
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
            if (UIHelper.IsPartialOverlap(item, _leftHandInventoryRect) && _handInventory.LeftHandSlot.IsEmpty())
                return true;
            if (UIHelper.IsPartialOverlap(item, _rightHandInventoryRect) && _handInventory.RightHandSlot.IsEmpty())
                return true;
            if (UIHelper.IsPartialOverlap(item, _leftPocketRect) && _handInventory.LeftPocketSlot.IsEmpty())
                return true;
            if (UIHelper.IsPartialOverlap(item, _rightPocketRect) && _handInventory.RightPocketSlot.IsEmpty())
                return true;
            return false;
        }

        public bool EquipFromInventoryToHand(InventoryItem item, RectTransform rectTransformItem)
        {
            bool equipped = false;
            if (UIHelper.IsPartialOverlap(rectTransformItem, _leftHandInventoryRect) && _handInventory.LeftHandSlot.IsEmpty())
                equipped = _handInventory.TryEquipToHand(item.ItemData, Hand.Left);
            else if (UIHelper.IsPartialOverlap(rectTransformItem, _rightHandInventoryRect) && _handInventory.RightHandSlot.IsEmpty())
                equipped = _handInventory.TryEquipToHand(item.ItemData, Hand.Right);
            else if (UIHelper.IsPartialOverlap(rectTransformItem, _leftPocketRect) && _handInventory.LeftPocketSlot.IsEmpty())
                equipped = _handInventory.TryEquipToPocket(item.ItemData, Hand.Left);
            else if (UIHelper.IsPartialOverlap(rectTransformItem, _rightPocketRect) && _handInventory.RightPocketSlot.IsEmpty())
                equipped = _handInventory.TryEquipToPocket(item.ItemData, Hand.Right);

            if (equipped)
            {
                RemoveItem(item);
            }

            return equipped;
        }

        // FOR TEST
        public bool TryAddItem(SO_ItemData itemData)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
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
