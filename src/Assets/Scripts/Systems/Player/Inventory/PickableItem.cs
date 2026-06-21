using NDTB.Core;
using NDTB.Data;
using UnityEngine;

namespace NDTB.Systems.Player.Inventory
{
    public class PickableItem : MonoBehaviour, IPicked
    {
        [field: SerializeField] public ItemData Item { get; set; }

        public void Pick()
        {
            Destroy(gameObject);
        }

        private void Start()
        {
            if (Item == null)
            {
                Debug.LogError("item can't be null");
                Destroy(gameObject);
            }
        }
    }
}
