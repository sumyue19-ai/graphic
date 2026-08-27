using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class AutoSnapToSocket : MonoBehaviour
{
    public enum EquipItemType { Helmet, Goggles, Gloves }

    [Header("Item Configuration")]
    [SerializeField] private EquipItemType itemType;
    [SerializeField] private Transform targetSocketTransform;
    [SerializeField] private SafetyStationFeedback stationFeedback;

    [Header("Goggles Specific")]
    [SerializeField] private GameObject visorTintObject;

    [Header("Gloves Specific")]
    [SerializeField] private GameObject rightGloveVisual; // Assign the right glove on RightHand Controller

    private XRGrabInteractable grabInteractable;
    private XRInteractionManager interactionManager;
    private bool isReady = false;
    private bool hasSnapped = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private IEnumerator Start()
    {
        interactionManager = FindObjectOfType<XRInteractionManager>();

        if (stationFeedback == null)
            stationFeedback = FindObjectOfType<SafetyStationFeedback>();

        yield return new WaitForSeconds(0.5f);
        isReady = true;
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!isReady || hasSnapped) return;
        if (args.interactorObject is XRSocketInteractor) return;

        StartCoroutine(LockItemToTarget(args.interactorObject));
    }

    private IEnumerator LockItemToTarget(IXRSelectInteractor handInteractor)
    {
        hasSnapped = true;

        // 1. Drop cleanly from hand controller first
        if (interactionManager != null && grabInteractable != null)
        {
            interactionManager.SelectExit(handInteractor, grabInteractable);
        }

        // CRITICAL FIX FOR GLOVES:
        // Wait an extra frame for the XR Toolkit to completely finish its internal 
        // physics evaluation loop before we violently override the Rigidbody settings.
        if (itemType == EquipItemType.Gloves)
        {
            yield return null;
        }

        yield return null;

        // 2. Disable grab logic so it stays locked
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // 3. Freeze Rigidbody safely
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 4. Disable physics colliders to prevent jitter
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // 5. Parent directly to target socket/controller
        if (targetSocketTransform != null)
        {
            Vector3 originalScale = transform.localScale;
            transform.SetParent(targetSocketTransform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = originalScale;
        }

        // 6. Activate Visor Tint if Goggles
        if (itemType == EquipItemType.Goggles && visorTintObject != null)
        {
            visorTintObject.SetActive(true);
        }

        // 7. Activate Right Glove if Gloves
        if (itemType == EquipItemType.Gloves && rightGloveVisual != null)
        {
            rightGloveVisual.SetActive(true);
        }

        // 8. Update billboard UI, particles, and audio
        if (stationFeedback != null)
        {
            stationFeedback.SetItemEquipped(itemType.ToString().ToLower(), true);
        }
    }
}
