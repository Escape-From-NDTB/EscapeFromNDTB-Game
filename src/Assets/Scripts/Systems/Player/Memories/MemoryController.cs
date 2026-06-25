using System.Collections.Generic;
using NDTB.Data;
using UnityEngine;
using UnityEngine.UI;
namespace NDTB.Systems.Player.Memories
{
    public class MemoryController : MonoBehaviour
    {

        [SerializeField] private readonly Memories _memories;

        [SerializeField] private GameObject _memoryUiPrefab;

        [SerializeField] private Transform _memoryUiParent;

        [SerializeField] private Text _memoryFullText;

        private readonly Dictionary<SO_Memory, GameObject> _memoryUiItems = new Dictionary<SO_Memory, GameObject>();

        private void Start()
        {
            if (_memories != null)
            {
                _memories.OnMemoryAdded += HandleMemoryAdded;
                _memories.OnMemoryRemoved += HandleMemoryRemoved;
                InitialPopulate();
            }
            else
            {
                Debug.LogError("Memories script not assigned to MemoryController.");
            }

            if (_memoryFullText != null)
            {
                _memoryFullText.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_memories != null)
            {
                _memories.OnMemoryAdded -= HandleMemoryAdded;
                _memories.OnMemoryRemoved -= HandleMemoryRemoved;
            }
        }

        private void InitialPopulate()
        {
            if (_memories != null)
            {
                foreach (SO_Memory memory in _memories.MemoriesList)
                {
                    HandleMemoryAdded(memory);
                }
            }
        }

        private void HandleMemoryAdded(SO_Memory memory)
        {
            if (_memoryUiPrefab != null && _memoryUiParent != null)
            {
                GameObject memoryUIObject = Instantiate(_memoryUiPrefab, _memoryUiParent);
                MemoryUIItem memoryUIItem = memoryUIObject.GetComponent<MemoryUIItem>();
                if (memoryUIItem != null)
                {
                    memoryUIItem.Setup(memory, this);
                    _memoryUiItems[memory] = memoryUIObject;
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

        private void HandleMemoryRemoved(SO_Memory memory)
        {
            if (_memoryUiItems.TryGetValue(memory, out GameObject memoryUIObject))
            {
                Destroy(memoryUIObject);
                _memoryUiItems.Remove(memory);
            }
        }

        public void ShowMemoryText(SO_Memory memory)
        {
            if (_memoryFullText != null)
            {
                _memoryFullText.gameObject.SetActive(true);
                _memoryFullText.text = memory.Text;
            }
        }
    }
}
