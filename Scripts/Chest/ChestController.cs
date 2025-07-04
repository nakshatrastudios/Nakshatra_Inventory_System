using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class ChestController : MonoBehaviour
    {
        [Header("UI Prompt")]
        public GameObject promptUI;
        public KeyCode openKey = KeyCode.E;
        public bool useSameKeyToClose = false;

        [Header("Chest UI")]
        [Tooltip("Assigned by CreateChestEditor")]
        public GameObject chestPanel;

        [Header("Objects To Toggle")]
        [Tooltip("Enabled when chest opens, disabled on close")]
        public List<GameObject> objectsToToggle = new List<GameObject>();

        [Header("Camera Control")]
        [Tooltip("Drag your camera-look script here")]
        public Behaviour cameraControl;

        [Header("Chest Animation")]
        public Transform lidTransform;
        public Vector3 openAngle = new Vector3(0f, 0f, 90f);
        public float animationDuration = 0.5f;

        [Header("Sound Effects")]
        public AudioClip openSound;
        public AudioClip closeSound;

        private AudioSource audioSource;
        private Quaternion closedRotation, openRotation;
        private bool isOpen = false, playerInRange = false;
        private Coroutine animationCoroutine;
        private Text promptText;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;

            if (lidTransform != null)
            {
                closedRotation = lidTransform.localRotation;
                openRotation = closedRotation * Quaternion.Euler(openAngle);
            }

            if (promptUI != null)
            {
                promptText = promptUI.GetComponent<Text>();
                promptUI.SetActive(false);
            }

            if (chestPanel != null)
                chestPanel.SetActive(false);

            foreach (var obj in objectsToToggle)
                if (obj != null) obj.SetActive(false);

            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void Update()
        {
            if (playerInRange && Input.GetKeyDown(openKey))
            {
                if (!isOpen)
                    OpenChest();
                else if (useSameKeyToClose)
                    CloseChest();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                if (promptUI != null)
                {
                    promptUI.SetActive(true);
                    promptText.text = $"Press {openKey} to {(isOpen ? "close" : "open")} chest";
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                if (promptUI != null) promptUI.SetActive(false);
                if (isOpen) CloseChest();
            }
        }

        private void OpenChest()
        {
            // hide prompt
            if (promptUI != null) promptUI.SetActive(false);

            // disable camera control
            if (cameraControl != null) cameraControl.enabled = false;

            // show UI & cursor
            if (chestPanel != null) chestPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // enable extra objects
            foreach (var obj in objectsToToggle)
                if (obj != null) obj.SetActive(true);

            // dispense currency now
            var inv = GetComponent<ChestInventory>();
            inv?.DispenseCurrency();

            // play sound & animate lid
            if (openSound != null) audioSource.PlayOneShot(openSound);
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(RotateLid(closedRotation, openRotation));

            isOpen = true;
        }

        private void CloseChest()
        {
            // hide UI & cursor
            if (chestPanel != null) chestPanel.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // re-enable camera
            if (cameraControl != null) cameraControl.enabled = true;

            // disable extra objects
            foreach (var obj in objectsToToggle)
                if (obj != null) obj.SetActive(false);

            // play sound & animate lid back
            if (closeSound != null) audioSource.PlayOneShot(closeSound);
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(RotateLid(openRotation, closedRotation));

            isOpen = false;
        }

        private IEnumerator RotateLid(Quaternion from, Quaternion to)
        {
            float t = 0f;
            while (t < animationDuration)
            {
                t += Time.deltaTime;
                float pct = Mathf.Clamp01(t / animationDuration);
                if (lidTransform != null)
                    lidTransform.localRotation = Quaternion.Slerp(from, to, pct);
                yield return null;
            }
            if (lidTransform != null) lidTransform.localRotation = to;
            animationCoroutine = null;
        }
    }
}