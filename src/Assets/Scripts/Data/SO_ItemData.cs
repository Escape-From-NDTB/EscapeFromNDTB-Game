using UnityEngine;

namespace NDTB.Data
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class SO_ItemData : ScriptableObject
    {
        [Header("Visual")]
        public string ItemName;
        public Sprite Icon;
        public Sprite InventoryTexture;
        public Mesh Mesh;
        public GameObject Prefab;
        [Header("Settings")]
        public int Height = 1;
        public int Width = 1;
        public bool IsTwoHanded = false;
        public float Weight;
    }
}
