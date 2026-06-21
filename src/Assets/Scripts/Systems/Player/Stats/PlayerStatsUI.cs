using UnityEngine;
using UnityEngine.UI;

namespace NDTB.Systems.Player.Stats
{
    public class PlayerStatsUI : MonoBehaviour
    {
        private PlayerStats player_Stats;

        public Slider HungerSlider;
        public Slider SanitySlider;
        public Slider ExplanatorySlider;

        private void Start()
        {
            if (player_Stats == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    player_Stats = player.GetComponent<PlayerStats>();
                }
            }
        }

        private void Update()
        {
            if (player_Stats != null)
            {
                if (HungerSlider != null)
                {
                    HungerSlider.value = player_Stats.CurrentHunger / player_Stats.MaxHunger;
                }

                if (SanitySlider != null)
                {
                    SanitySlider.value = player_Stats.CurrentSanity / player_Stats.MaxSanity;
                }

                if (ExplanatorySlider != null)
                {
                    ExplanatorySlider.value = player_Stats.CurrentExplanatory / player_Stats.MaxExplanatory;
                }
            }
        }
    }
}
