using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class SafetyStationFeedback : MonoBehaviour
{
    [Header("UI Checklist Elements")]
    public TextMeshProUGUI helmetStatusText;
    public TextMeshProUGUI gogglesStatusText;
    public TextMeshProUGUI glovesStatusText;

    [Header("Glove Visual (Right Hand)")]
    public GameObject rightGloveVisual;

    [Header("Table Item (Left Hand Attachment)")]
    public GameObject itemGloves;
    public XRSocketInteractor socketGloves;

    [Header("Transform Targets for Particles & Audio")]
    public Transform headTarget;
    public Transform faceTarget;
    public Transform handsTarget;

    [Header("Feedback Effects")]
    public ParticleSystem successParticles;
    public AudioSource audioSource;
    public AudioClip equipSound;

    // Modern XR Toolkit 2.x/3.x event hooks
    public void OnHelmetSelectEntered(SelectEnterEventArgs args) => SetItemEquipped("helmet", true);
    public void OnGogglesSelectEntered(SelectEnterEventArgs args) => SetItemEquipped("goggles", true);
    public void OnGlovesSelectEntered(SelectEnterEventArgs args) => SetItemEquipped("gloves", true);

    // Inspector helper functions
    public void EquipHelmet() => SetItemEquipped("helmet", true);
    public void UnequipHelmet() => SetItemEquipped("helmet", false);

    public void EquipGoggles() => SetItemEquipped("goggles", true);
    public void UnequipGoggles() => SetItemEquipped("goggles", false);

    public void EquipGloves() => SetItemEquipped("gloves", true);
    public void UnequipGloves() => SetItemEquipped("gloves", false);

    public void SetItemEquipped(string itemType, bool isEquipped)
    {
        // Fully ASCII-compatible formatted indicator (prevents Unicode \u2713 font error)
        string status = isEquipped ? "<color=#00FF00>[OK]</color> " : "<color=#AAAAAA>[  ]</color> ";
        Transform spawnPoint = null;

        switch (itemType.ToLower())
        {
            case "helmet":
                if (helmetStatusText != null)
                    helmetStatusText.text = status + "Hard Hat Equipped";
                spawnPoint = headTarget;
                break;

            case "goggles":
                if (gogglesStatusText != null)
                    gogglesStatusText.text = status + "Safety Goggles Equipped";
                spawnPoint = faceTarget;
                break;

            case "gloves":
                if (glovesStatusText != null)
                    glovesStatusText.text = status + "Safety Gloves Equipped";
                spawnPoint = handsTarget;

                // 1. Show Right Glove on right hand
                if (rightGloveVisual != null)
                    rightGloveVisual.SetActive(true);

                // 2. Lock Left Glove into socket permanently
                if (isEquipped)
                {
                    StartCoroutine(LockGlovesPermanently());
                }
                break;
        }

        if (isEquipped)
        {
            if (spawnPoint != null)
            {
                if (successParticles != null)
                {
                    successParticles.transform.position = spawnPoint.position;
                    successParticles.Play();
                }

                if (audioSource != null && equipSound != null)
                {
                    audioSource.transform.position = spawnPoint.position;
                    audioSource.PlayOneShot(equipSound);
                }
            }
            else
            {
                if (successParticles != null)
                    successParticles.Play();

                if (audioSource != null && equipSound != null)
                    audioSource.PlayOneShot(equipSound);
            }
        }
    }

    private IEnumerator LockGlovesPermanently()
    {
        yield return null;

        if (itemGloves != null)
        {
            XRGrabInteractable grab = itemGloves.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = false;

            Rigidbody rb = itemGloves.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Collider[] colliders = itemGloves.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            if (socketGloves != null)
            {
                itemGloves.transform.SetParent(socketGloves.transform.parent, true);
                itemGloves.transform.position = socketGloves.transform.position;
                itemGloves.transform.rotation = socketGloves.transform.rotation;
            }
        }

        if (socketGloves != null)
        {
            socketGloves.enabled = false;
        }
    }

    public void ResetStation()
    {
        UnequipHelmet();
        UnequipGoggles();
        UnequipGloves();
    }
}