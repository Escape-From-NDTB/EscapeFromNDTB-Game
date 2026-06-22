using NDTB.Core;
using UnityEngine;

namespace NDTB.Systems.Player
{
    public class Abnormal : MonoBehaviour, IAbnormal
    {
        [Range(0, 10)][SerializeField] private float level;
        public float Level => level;
    }
}
