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
    }

    public List<ResetObject> objectsToReset = new List<ResetObject>();

    public GameObject boxBlocker;

    // The magnet's socket
    public XRSocketInteractor magnetSocket;

    private bool boxBlockerStartState;

    private void Start()
    {
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            obj.startPosition = obj.target.transform.position;
            obj.startRotation = obj.target.transform.rotation;
            obj.startActive = obj.target.activeSelf;
        }

        if (boxBlocker != null)
        {
            boxBlockerStartState = boxBlocker.activeSelf;
        }
    }

    public void ResetTraining()
    {
        // IMPORTANT:
        // Release anything currently attached to the magnet socket.
        if (magnetSocket != null)
        {
            var interactable = magnetSocket.GetOldestInteractableSelected();

            if (interactable != null)
            {
                magnetSocket.interactionManager.SelectExit(
                    magnetSocket,
                    interactable
                );
            }
        }

        // Now reset all objects.
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            obj.target.SetActive(obj.startActive);

            obj.target.transform.position = obj.startPosition;
            obj.target.transform.rotation = obj.startRotation;

            Rigidbody rb = obj.target.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        if (boxBlocker != null)
        {
            boxBlocker.SetActive(boxBlockerStartState);
        }
    }
}