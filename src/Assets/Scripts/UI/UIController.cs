using NDTB.Systems.Player;
using UnityEngine;
using UnityEngine.InputSystem;
namespace NDTB.UI
{
    public class UIController : MonoBehaviour
    {
        private CameraController _cameraController;
        private GameObject _activeMenu = null;

        [SerializeField] private GameObject _baseUi;

        [SerializeField] private GameObject[] _menus;

        [SerializeField] private InputActionReference[] _menuActions;

        [SerializeField] private InputActionReference _pauseAction;

        private void OnEnable()
        {
            foreach (var actionRef in _menuActions)
            {
                if (actionRef != null)
                    actionRef.action.Enable();
            }
            if (_pauseAction != null)
                _pauseAction.action.Enable();
        }

        private void OnDisable()
        {
            foreach (var actionRef in _menuActions)
            {
                if (actionRef != null)
                    actionRef.action.Disable();
            }
            if (_pauseAction != null)
                _pauseAction.action.Disable();
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _cameraController = player.GetComponent<CameraController>();
            }
            foreach (var menu in _menus)
            {
                menu.SetActive(false);
            }
        }

        private void Update()
        {
            bool pauseHandled = false;

            for (int i = 0; i < _menuActions.Length; i++)
            {
                if (_menuActions[i] != null && _menuActions[i].action.WasPressedThisFrame())
                {
                    if (_menus[i] == _activeMenu)
                    {
                        _activeMenu.SetActive(false);
                        _baseUi.SetActive(true);
                        _activeMenu = null;
                        _cameraController.Activate();
                        if (_menuActions[i] == _pauseAction) pauseHandled = true;
                        continue;
                    }

                    if (_activeMenu != null)
                        continue;

                    _activeMenu = _menus[i];
                    _activeMenu.SetActive(true);
                    _baseUi.SetActive(false);
                    _cameraController.Deactivate();
                    if (_menuActions[i] == _pauseAction) pauseHandled = true;
                }
            }

            if (!pauseHandled && _pauseAction != null && _pauseAction.action.WasPressedThisFrame() && _activeMenu != null)
            {
                _activeMenu.SetActive(false);
                _baseUi.SetActive(true);
                _activeMenu = null;
                _cameraController.Activate();
            }
        }
    }
}
