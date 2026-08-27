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

    [Header("Console Barrier & Warning Interlock")]
    public GameObject consoleBarrier;
    public GameObject panelWarning;

    [Header("Glove Visuals")]
    public GameObject rightGloveVisual; // Under RightHand Controller
    public GameObject leftGloveVisual;  // Under LeftHand Controller

    [Header("Table Item Display")]
    public GameObject itemGloves; // The physical item on the desk station

    [Header("Transform Targets for Particles & Audio")]
    public Transform headTarget;
    public Transform faceTarget;
    public Transform handsTarget;

    [Header("Feedback Effects")]
    public ParticleSystem successParticles;
    public AudioSource audioSource;
    public AudioClip equipSound;

    // Internal state tracking for machinery interlock
    private bool isHelmetEquipped = false;
    private bool isGogglesEquipped = false;
    private bool isGlovesEquipped = false;

    private void Start()
    {
        if (consoleBarrier != null) consoleBarrier.SetActive(true);
        if (panelWarning != null) panelWarning.SetActive(true);
        if (rightGloveVisual != null) rightGloveVisual.SetActive(false);
        if (leftGloveVisual != null) leftGloveVisual.SetActive(false);
    }

    // Modern XR Toolkit 2.x/3.x event hooks
    public void OnHelmetSelectEntered(SelectEnterEventArgs args) => SetItemEquipped("helmet", true, args.interactorObject.transform);
    public void OnGogglesSelectEntered(SelectEnterEventArgs args) => SetItemEquipped("goggles", true, args.interactorObject.transform);
    public void OnGlovesSelectEntered(SelectEnterEventArgs args) => SetItemEquipped("gloves", true, args.interactorObject.transform);

    // Inspector helper functions
    public void EquipHelmet() => SetItemEquipped("helmet", true);
    public void UnequipHelmet() => SetItemEquipped("helmet", false);

    public void EquipGoggles() => SetItemEquipped("goggles", true);
    public void UnequipGoggles() => SetItemEquipped("goggles", false);

    public void EquipGloves() => SetItemEquipped("gloves", true);
    public void UnequipGloves() => SetItemEquipped("gloves", false);

    public void SetItemEquipped(string itemType, bool isEquipped)
    {
        SetItemEquipped(itemType, isEquipped, null);
    }

    public void SetItemEquipped(string itemType, bool isEquipped, Transform interactorTransform)
    {
        string status = isEquipped ? "<color=#00FF00>[OK]</color> " : "<color=#AAAAAA>[  ]</color> ";
        Transform spawnPoint = null;

        switch (itemType.ToLower())
        {
            case "helmet":
                isHelmetEquipped = isEquipped;
                if (helmetStatusText != null)
                    helmetStatusText.text = status + "Hard Hat Equipped";
                spawnPoint = headTarget != null ? headTarget : interactorTransform;
                break;

            case "goggles":
                isGogglesEquipped = isEquipped;
                if (gogglesStatusText != null)
                    gogglesStatusText.text = status + "Safety Goggles Equipped";
                spawnPoint = faceTarget != null ? faceTarget : interactorTransform;
                break;

            case "gloves":
                isGlovesEquipped = isEquipped;
                if (glovesStatusText != null)
                    glovesStatusText.text = status + "Safety Gloves Equipped";
                spawnPoint = handsTarget != null ? handsTarget : interactorTransform;

                // 1. Turn on Right Controller visual asset
                if (rightGloveVisual != null)
                    rightGloveVisual.SetActive(isEquipped);

                // 2. Turn on Left Controller visual asset 
                if (leftGloveVisual != null)
                    leftGloveVisual.SetActive(isEquipped);

                // 3. Cleanly clear the table item out of sight
                if (isEquipped && itemGloves != null)
                {
                    StartCoroutine(HideTableItemCleanly());
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

            CheckAllEquipped();
        }
    }

    private void CheckAllEquipped()
    {
        if (isHelmetEquipped && isGogglesEquipped && isGlovesEquipped)
        {
            if (consoleBarrier != null)
                consoleBarrier.SetActive(false);

            if (panelWarning != null)
                panelWarning.SetActive(false);

            Debug.Log("All PPE equipped! Console barrier removed.");
        }
    }

    private IEnumerator HideTableItemCleanly()
    {
        // Wait exactly 1 frame for XRI to finish its internal pickup evaluation cycle
        yield return null;

        if (itemGloves != null)
        {
            Rigidbody rb = itemGloves.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Clear any residual movement forces BEFORE turning off the object
                // This completely suppresses the yellow "Setting velocity of a kinematic body" warning logs
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // Turn it completely off so it hides from the desk area instantly
            itemGloves.SetActive(false);
        }
    }

    public void ResetStation()
    {
        UnequipHelmet();
        UnequipGoggles();
        UnequipGloves();

        if (rightGloveVisual != null) rightGloveVisual.SetActive(false);
        if (leftGloveVisual != null) leftGloveVisual.SetActive(false);

        if (itemGloves != null)
        {
            Rigidbody rb = itemGloves.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Safely restore physics options when resetting the item back to the table
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            itemGloves.SetActive(true);
        }

        if (consoleBarrier != null) consoleBarrier.SetActive(true);
        if (panelWarning != null) panelWarning.SetActive(true);
    }
}
