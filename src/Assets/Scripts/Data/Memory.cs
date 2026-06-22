using UnityEngine;

namespace NDTB.Data
{
    [CreateAssetMenu(fileName = "New Memory", menuName = "Memories/Memory")]
    public class Memory : ScriptableObject
    {
        public string Name;
        public string Description;
        public string Text;
        public Sprite Icon;
    }
}
