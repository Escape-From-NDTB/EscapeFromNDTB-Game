using NDTB.Core;
using NDTB.Data;
using UnityEngine;

namespace NDTB.Systems.Player.Memories
{
    public class MemoryableItem : MonoBehaviour, IMemoryable
    {
        [field: SerializeField] public SO_Memory Memory { get; set; }

        public void Save()
        {

        }

        private void Start()
        {
            if (Memory == null)
            {
                Debug.LogError("item can't be null");
                Destroy(gameObject);
            }
        }
    }
}
