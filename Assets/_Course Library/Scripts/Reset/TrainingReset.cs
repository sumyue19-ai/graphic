using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TrainingReset : MonoBehaviour
{
    [System.Serializable]
    public class ResetObject
    {
        public GameObject target;

        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public Quaternion startRotation;
        [HideInInspector] public bool startActive;

        [HideInInspector] public bool hasRigidbody;
        [HideInInspector] public bool startIsKinematic;
        [HideInInspector] public bool startUseGravity;
        [HideInInspector] public RigidbodyConstraints startConstraints;
    }

    public List<ResetObject> objectsToReset = new List<ResetObject>();

    public GameObject boxBlocker;
    public XRSocketInteractor magnetSocket;

    private bool boxBlockerStartState;
    private bool magnetSocketStartEnabled;

    private void Start()
    {
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            obj.startPosition = obj.target.transform.position;
            obj.startRotation = obj.target.transform.rotation;
            obj.startActive = obj.target.activeSelf;

            Rigidbody rb = obj.target.GetComponent<Rigidbody>();

            if (rb != null)
            {
                obj.hasRigidbody = true;
                obj.startIsKinematic = rb.isKinematic;
                obj.startUseGravity = rb.useGravity;
                obj.startConstraints = rb.constraints;
            }
        }

        if (boxBlocker != null)
        {
            boxBlockerStartState = boxBlocker.activeSelf;
        }

        if (magnetSocket != null)
        {
            magnetSocketStartEnabled = magnetSocket.enabled;
        }
    }

    public void ResetTraining()
    {
        StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        // 1. Release anything held by magnet
        if (magnetSocket != null)
        {
            List<IXRSelectInteractable> selectedObjects =
                new List<IXRSelectInteractable>(magnetSocket.interactablesSelected);

            foreach (IXRSelectInteractable interactable in selectedObjects)
            {
                if (interactable != null)
                {
                    magnetSocket.interactionManager.SelectExit(
                        magnetSocket,
                        interactable
                    );
                }
            }

            magnetSocket.enabled = false;
        }

        yield return null;

        // 2. Freeze rigidbodies first
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            Rigidbody rb = obj.target.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        // 3. Restore positions and rotations
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            obj.target.SetActive(obj.startActive);

            obj.target.transform.position = obj.startPosition;
            obj.target.transform.rotation = obj.startRotation;
        }

        // 4. Restore BoxBlocker
        if (boxBlocker != null)
        {
            boxBlocker.SetActive(boxBlockerStartState);
        }

        // Let Unity apply the new transforms
        yield return new WaitForFixedUpdate();
        yield return null;

        // 5. Restore original Rigidbody settings
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            Rigidbody rb = obj.target.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = obj.startIsKinematic;
                rb.useGravity = obj.startUseGravity;
                rb.constraints = obj.startConstraints;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 6. Turn magnet socket back on
        if (magnetSocket != null)
        {
            magnetSocket.enabled = magnetSocketStartEnabled;
        }
    }
}