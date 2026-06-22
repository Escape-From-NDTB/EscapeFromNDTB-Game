using NDTB.Core;
using UnityEngine;

namespace NDTB.Systems.Player.Stats
{
    public class Abnormal : MonoBehaviour, IAbnormal
    {
        [Range(0, 10)][SerializeField] private float _level;
        public float Level => _level;
    }
}
