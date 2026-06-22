using UnityEngine;

namespace NDTB.Core
{
    public abstract class FocusableObject : MonoBehaviour, IFocusable
    {
        public void OnBeginFocus() { }
        public void OnStartFocus() { }
        public void OnEndFocus() { }
    }
}
