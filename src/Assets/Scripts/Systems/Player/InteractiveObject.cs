using NDTB.Core;
using UnityEngine;

namespace NDTB.Systems.Player
{
    public abstract class InteractiveObject : MonoBehaviour, IInteractive
    {
        [field: SerializeField] public string Name { get; set; }

        public abstract void Interact();
    }
}
