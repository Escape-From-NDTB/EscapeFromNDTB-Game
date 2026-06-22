using NDTB.Data;
using UnityEngine;
using UnityEngine.UI;
namespace NDTB.Systems.Player.Memories
{
    [RequireComponent(typeof(Button))]
    public class MemoryUIItem : MonoBehaviour
    {
        public SO_Memory Memory { get; private set; }
        private MemoryController _memoryController;

        [SerializeField] private Text _nameText;

        [SerializeField] private Text _descriptionText;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnMemoryClicked);
        }

        public void Setup(SO_Memory memoryToSetup, MemoryController controller)
        {
            this.Memory = memoryToSetup;
            this._memoryController = controller;

            if (_nameText != null) _nameText.text = memoryToSetup.Name;
            if (_descriptionText != null) _descriptionText.text = memoryToSetup.Description;

            gameObject.name = $"Memory - {memoryToSetup.Name}";
        }

        private void OnMemoryClicked()
        {
            if (_memoryController != null)
            {
                _memoryController.ShowMemoryText(Memory);
            }
        }
    }
}
