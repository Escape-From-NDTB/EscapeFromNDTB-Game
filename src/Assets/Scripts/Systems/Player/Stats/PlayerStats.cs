using System.Linq;
using NDTB.Core;
using UnityEngine;

namespace NDTB.Systems.Player.Stats
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerStats : MonoBehaviour
    {
        [Header("Max Stats")]
        public float MaxHunger = 100f;
        public float MaxSanity = 100f;
        public int MaxExplanatory = 3;

        [Header("Current Stats")]
        [SerializeField] private float current_Hunger;
        [SerializeField] private float current_Sanity;
        [SerializeField] private float current_Explanatory;

        public float CurrentHunger => current_Hunger;
        public float CurrentSanity => current_Sanity;
        public float CurrentExplanatory => current_Explanatory;

        [Header("Base Decay Rates (per second)")]
        public float HungerDecayRate = 0.5f;
        public float SanityDecayRate = 0.1f;

        [Header("State Multipliers")]
        public float RunHungerMultiplier = 2f;
        public float CrouchHungerMultiplier = 0.5f;

        [Header("Action Costs")]
        public float JumpHungerCost = 1f;

        private IAbnormal[] abnormals;
        private PlayerMovement player_Movement;
        private new Camera camera;

        private void Start()
        {
            player_Movement = GetComponent<PlayerMovement>();

            current_Hunger = MaxHunger;
            current_Sanity = MaxSanity;
            current_Explanatory = MaxExplanatory;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                camera = player.GetComponentInChildren<Camera>();
                if (camera == null)
                {
                    Debug.LogWarning("[PlayerStats] Could not find Camera in children of Player object. Using Camera.main as fallback.");
                    camera = Camera.main;
                }
            }
            else
            {
                Debug.LogError("[PlayerStats] Player object with tag 'Player' not found! Using Camera.main as fallback.");
                camera = Camera.main;
            }

            if (camera == null)
            {
                Debug.LogError("[PlayerStats] No camera found (neither child of Player nor MainCamera). Abnormal effects will not work.");
            }

            abnormals = FindObjectsByType<MonoBehaviour>().OfType<IAbnormal>().ToArray();
        }

        private void Update()
        {
            if (player_Movement.JumpedThisFrame)
            {
                ChangeHunger(-JumpHungerCost);
            }

            var currentHungerDecay = HungerDecayRate;
            var currentSanityDecay = SanityDecayRate;

            if (player_Movement.IsRunning)
            {
                currentHungerDecay *= RunHungerMultiplier;
            }
            else if (player_Movement.IsCrouching)
            {
                currentHungerDecay *= CrouchHungerMultiplier;
            }

            var abnormalMultiplier = 0f;
            if (camera != null && abnormals != null)
            {
                foreach (var abnormal in abnormals)
                {
                    var monoBehaviour = abnormal as MonoBehaviour;
                    if (monoBehaviour == null || !monoBehaviour.isActiveAndEnabled) continue;

                    var abnormalRenderer = monoBehaviour.GetComponent<Renderer>();
                    if (abnormalRenderer != null && IsVisible(abnormalRenderer))
                    {
                        abnormalMultiplier += abnormal.Level;
                    }
                }
            }

            currentSanityDecay *= 1 + abnormalMultiplier;

            ChangeHunger(-currentHungerDecay * Time.deltaTime);
            ChangeSanity(-currentSanityDecay * Time.deltaTime);
        }

        private bool IsVisible(Renderer renderer)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (!GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            {
                return false;
            }

            Vector3 direction = renderer.bounds.center - camera.transform.position;
            if (Physics.Raycast(camera.transform.position, direction, out var hit, direction.magnitude))
            {
                if (hit.collider.gameObject == renderer.gameObject)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public void ChangeHunger(float amount)
        {
            current_Hunger += amount;
            current_Hunger = Mathf.Clamp(current_Hunger, 0, MaxHunger);
        }

        public void ChangeSanity(float amount)
        {
            current_Sanity += amount;
            current_Sanity = Mathf.Clamp(current_Sanity, 0, MaxSanity);
        }

        public void ChangeExplanatory(float amount)
        {
            current_Explanatory += amount;
            current_Explanatory = Mathf.Clamp(current_Explanatory, 0, MaxExplanatory);
        }
    }
}
