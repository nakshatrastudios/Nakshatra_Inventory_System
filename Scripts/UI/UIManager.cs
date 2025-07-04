using System.Collections.Generic;
using UnityEngine;

namespace Nakshatra.Plugins
{
    public class UIManager : MonoBehaviour
    {
        [System.Serializable]
        public class CanvasInfo
        {
            public string name;
            public GameObject canvas;
        }

        [Header("Inventory Panel Toggles")]
        public KeyCode openKey = KeyCode.I;
        public KeyCode closeKey = KeyCode.Escape;

        [Header("Time Control")]
        public bool freezeTimeOnOpen = false;

        [Header("Canvases to enable when opening Inventory")]
        public List<CanvasInfo> canvasesToEnableOnOpen = new List<CanvasInfo>();
        [Header("Canvases to enable when closing Inventory")]
        public List<CanvasInfo> canvasesToEnableOnClose = new List<CanvasInfo>();

        [Header("GameObjects to disable when opening Inventory")]
        public List<GameObject> gameObjectsToDisableOnOpen = new List<GameObject>();

        [Header("Camera Controller (will be disabled when time freezes)")]
        public CameraController cameraController;

        private bool isOpen = false;
        private float previousTimeScale = 1f;

        void Start()
        {
            ApplyState(isOpen);
            LockCursor();
        }

        void Update()
        {
            if (!isOpen && Input.GetKeyDown(openKey))
            {
                isOpen = true;
                ApplyState(isOpen);
            }
            else if (isOpen && Input.GetKeyDown(closeKey))
            {
                isOpen = false;
                ApplyState(isOpen);
            }
        }

        private void ApplyState(bool open)
        {
            // disable specified GameObjects when opening inventory (no re-enable on close)
            if (open)
            {
                foreach (var go in gameObjectsToDisableOnOpen)
                {
                    if (go)
                        go.SetActive(false);
                }
            }

            // toggle your canvases
            foreach (var info in canvasesToEnableOnOpen)
                if (info.canvas) info.canvas.SetActive(open);

            foreach (var info in canvasesToEnableOnClose)
                if (info.canvas) info.canvas.SetActive(!open);

            // cursor lock/unlock
            if (open) UnlockCursor();
            else     LockCursor();

            // time freeze + camera
            if (freezeTimeOnOpen)
            {
                if (open)
                {
                    previousTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                    if (cameraController != null)
                        cameraController.enabled = false;
                }
                else
                {
                    Time.timeScale = previousTimeScale;
                    if (cameraController != null)
                        cameraController.enabled = true;
                }
            }
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }
}
