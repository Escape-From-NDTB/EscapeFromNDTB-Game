using NDTB.Core;
using UnityEngine;

namespace NDTB.Systems.Player
{
    public abstract class FocusableObject : MonoBehaviour, IFocusable
    {
        public void OnBeginFocus() { }
        public void OnStartFocus() { }
        public void OnEndFocus() { }
    }
}
