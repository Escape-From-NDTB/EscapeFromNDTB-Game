using NDTB.Systems.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NDTB.UI
{
    public class UIController : MonoBehaviour
    {
        private CameraController camera_Controller;
        private GameObject active_Menu = null;

        [SerializeField] private GameObject base_UI;
        [SerializeField] private GameObject[] menus;
        [SerializeField] private InputActionReference[] menuActions;
        [SerializeField] private InputActionReference pauseAction;

        private void OnEnable()
        {
            foreach (var actionRef in menuActions)
            {
                if (actionRef != null)
                    actionRef.action.Enable();
            }
            if (pauseAction != null)
                pauseAction.action.Enable();
        }

        private void OnDisable()
        {
            foreach (var actionRef in menuActions)
            {
                if (actionRef != null)
                    actionRef.action.Disable();
            }
            if (pauseAction != null)
                pauseAction.action.Disable();
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                camera_Controller = player.GetComponent<CameraController>();
            }
            foreach (var menu in menus)
            {
                menu.SetActive(false);
            }
        }

        private void Update()
        {
            bool pauseHandled = false;

            for (int i = 0; i < menuActions.Length; i++)
            {
                if (menuActions[i] != null && menuActions[i].action.WasPressedThisFrame())
                {
                    if (menus[i] == active_Menu)
                    {
                        active_Menu.SetActive(false);
                        base_UI.SetActive(true);
                        active_Menu = null;
                        camera_Controller.Activate();
                        if (menuActions[i] == pauseAction) pauseHandled = true;
                        continue;
                    }

                    if (active_Menu != null)
                        continue;

                    active_Menu = menus[i];
                    active_Menu.SetActive(true);
                    base_UI.SetActive(false);
                    camera_Controller.Deactivate();
                    if (menuActions[i] == pauseAction) pauseHandled = true;
                }
            }

            if (!pauseHandled && pauseAction != null && pauseAction.action.WasPressedThisFrame() && active_Menu != null)
            {
                active_Menu.SetActive(false);
                base_UI.SetActive(true);
                active_Menu = null;
                camera_Controller.Activate();
            }
        }
    }
}
