using UnityEngine;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Stats
{
    public class PlayerStatsUI : MonoBehaviour
    {
        private PlayerStats _playerStats;

        public Slider HungerSlider;
        public Slider SanitySlider;
        public Slider ExplanatorySlider;

        private void Start()
        {
            if (_playerStats == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _playerStats = player.GetComponent<PlayerStats>();
                }
            }
        }

        private void Update()
        {
            if (_playerStats != null)
            {
                if (HungerSlider != null)
                {
                    HungerSlider.value = _playerStats.CurrentHunger / _playerStats.MaxHunger;
                }

                if (SanitySlider != null)
                {
                    SanitySlider.value = _playerStats.CurrentSanity / _playerStats.MaxSanity;
                }

                if (ExplanatorySlider != null)
                {
                    ExplanatorySlider.value = _playerStats.CurrentExplanatory / _playerStats.MaxExplanatory;
                }
            }
        }
    }
}
