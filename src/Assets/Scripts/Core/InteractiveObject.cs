using UnityEngine;

namespace NDTB.Core
{
    public abstract class InteractiveObject : MonoBehaviour, IInteractive
    {
        [field: SerializeField] public string Name { get; set; }

        public abstract void Interact();
    }
}
