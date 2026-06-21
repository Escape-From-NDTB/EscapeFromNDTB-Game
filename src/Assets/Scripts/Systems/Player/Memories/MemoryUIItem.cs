using NDTB.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Memories
{
    [RequireComponent(typeof(Button))]
    public class MemoryUIItem : MonoBehaviour
    {
        public Memory Memory { get; private set; }
        private MemoryController memory_Controller;

        [SerializeField] private Text name_Text;
        [SerializeField] private Text description_Text;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnMemoryClicked);
        }

        public void Setup(Memory memoryToSetup, MemoryController controller)
        {
            this.Memory = memoryToSetup;
            this.memory_Controller = controller;

            if (name_Text != null) name_Text.text = memoryToSetup.Name;
            if (description_Text != null) description_Text.text = memoryToSetup.Description;

            gameObject.name = $"Memory - {memoryToSetup.Name}";
        }

        private void OnMemoryClicked()
        {
            if (memory_Controller != null)
            {
                memory_Controller.ShowMemoryText(Memory);
            }
        }
    }
}
