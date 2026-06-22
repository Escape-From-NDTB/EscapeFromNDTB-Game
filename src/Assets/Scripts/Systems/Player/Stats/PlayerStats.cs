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
        [SerializeField] private float _currentHunger;

        [SerializeField] private float _currentSanity;

        [SerializeField] private float _currentExplanatory;

        public float CurrentHunger => _currentHunger;
        public float CurrentSanity => _currentSanity;
        public float CurrentExplanatory => _currentExplanatory;

        [Header("Base Decay Rates (per second)")]
        public float HungerDecayRate = 0.5f;
        public float SanityDecayRate = 0.1f;

        [Header("State Multipliers")]
        public float RunHungerMultiplier = 2f;
        public float CrouchHungerMultiplier = 0.5f;

        [Header("Action Costs")]
        public float JumpHungerCost = 1f;

        private IAbnormal[] _abnormals;
        private PlayerMovement _playerMovement;
        private new Camera _camera;

        private void Start()
        {
            _playerMovement = GetComponent<PlayerMovement>();

            _currentHunger = MaxHunger;
            _currentSanity = MaxSanity;
            _currentExplanatory = MaxExplanatory;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _camera = player.GetComponentInChildren<Camera>();
                if (_camera == null)
                {
                    Debug.LogWarning("[PlayerStats] Could not find Camera in children of Player object. Using Camera.main as fallback.");
                    _camera = Camera.main;
                }
            }
            else
            {
                Debug.LogError("[PlayerStats] Player object with tag 'Player' not found! Using Camera.main as fallback.");
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                Debug.LogError("[PlayerStats] No _camera found (neither child of Player nor MainCamera). Abnormal effects will not work.");
            }

            _abnormals = FindObjectsByType<MonoBehaviour>().OfType<IAbnormal>().ToArray();
        }

        private void Update()
        {
            if (_playerMovement.JumpedThisFrame)
            {
                ChangeHunger(-JumpHungerCost);
            }

            var currentHungerDecay = HungerDecayRate;
            var currentSanityDecay = SanityDecayRate;

            if (_playerMovement.IsRunning)
            {
                currentHungerDecay *= RunHungerMultiplier;
            }
            else if (_playerMovement.IsCrouching)
            {
                currentHungerDecay *= CrouchHungerMultiplier;
            }

            var abnormalMultiplier = 0f;
            if (_camera != null && _abnormals != null)
            {
                foreach (var abnormal in _abnormals)
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
            var planes = GeometryUtility.CalculateFrustumPlanes(_camera);
            if (!GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            {
                return false;
            }

            Vector3 direction = renderer.bounds.center - _camera.transform.position;
            if (Physics.Raycast(_camera.transform.position, direction, out var hit, direction.magnitude))
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
            _currentHunger += amount;
            _currentHunger = Mathf.Clamp(_currentHunger, 0, MaxHunger);
        }

        public void ChangeSanity(float amount)
        {
            _currentSanity += amount;
            _currentSanity = Mathf.Clamp(_currentSanity, 0, MaxSanity);
        }

        public void ChangeExplanatory(float amount)
        {
            _currentExplanatory += amount;
            _currentExplanatory = Mathf.Clamp(_currentExplanatory, 0, MaxExplanatory);
        }
    }
}
