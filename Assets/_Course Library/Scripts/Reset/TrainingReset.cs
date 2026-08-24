using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        // Save the original state when Play Mode starts
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            obj.startPosition = obj.target.transform.position;
            obj.startRotation = obj.target.transform.rotation;
            obj.startActive = obj.target.activeSelf;
        }
    }

    public void ResetTraining()
    {
        foreach (ResetObject obj in objectsToReset)
        {
            if (obj.target == null)
                continue;

            // Restore active state
            obj.target.SetActive(obj.startActive);

            // Restore position and rotation
            obj.target.transform.position = obj.startPosition;
            obj.target.transform.rotation = obj.startRotation;

            // Stop physics movement
            Rigidbody rb = obj.target.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}