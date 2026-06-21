using System.Collections.Generic;
using NDTB.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Memories
{
    public class MemoryController : MonoBehaviour
    {
        [SerializeField] private Memories memories;
        [SerializeField] private GameObject memory_UI_Prefab;
        [SerializeField] private Transform memory_UI_Parent;
        [SerializeField] private Text memory_Full_Text;

        private Dictionary<Memory, GameObject> memory_UI_Items = new Dictionary<Memory, GameObject>();

        private void Start()
        {
            if (memories != null)
            {
                memories.OnMemoryAdded += HandleMemoryAdded;
                memories.OnMemoryRemoved += HandleMemoryRemoved;
                InitialPopulate();
            }
            else
            {
                Debug.LogError("Memories script not assigned to MemoryController.");
            }

            if (memory_Full_Text != null)
            {
                memory_Full_Text.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (memories != null)
            {
                memories.OnMemoryAdded -= HandleMemoryAdded;
                memories.OnMemoryRemoved -= HandleMemoryRemoved;
            }
        }

        private void InitialPopulate()
        {
            if (memories != null)
            {
                foreach (Memory memory in memories.MemoriesList)
                {
                    HandleMemoryAdded(memory);
                }
            }
        }

        private void HandleMemoryAdded(Memory memory)
        {
            if (memory_UI_Prefab != null && memory_UI_Parent != null)
            {
                GameObject memoryUIObject = Instantiate(memory_UI_Prefab, memory_UI_Parent);
                MemoryUIItem memoryUIItem = memoryUIObject.GetComponent<MemoryUIItem>();
                if (memoryUIItem != null)
                {
                    memoryUIItem.Setup(memory, this);
                    memory_UI_Items[memory] = memoryUIObject;
                }
                else
                {
                    Debug.LogError("MemoryUIPrefab does not have a MemoryUIItem component.");
                }
            }
            else
            {
                Debug.LogWarning("MemoryUIPrefab or MemoryUIParent is not assigned in MemoryController.");
            }
        }

        private void HandleMemoryRemoved(Memory memory)
        {
            if (memory_UI_Items.TryGetValue(memory, out GameObject memoryUIObject))
            {
                Destroy(memoryUIObject);
                memory_UI_Items.Remove(memory);
            }
        }

        public void ShowMemoryText(Memory memory)
        {
            if (memory_Full_Text != null)
            {
                memory_Full_Text.gameObject.SetActive(true);
                memory_Full_Text.text = memory.Text;
            }
        }
    }
}
