using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class SafetyEquipmentStation : MonoBehaviour
{
    [Header("Equipment Sockets (XRSocketInteractor)")]
    public XRSocketInteractor helmetSocket;
    public XRSocketInteractor gogglesSocket;
    public XRSocketInteractor glovesSocket;

    [Header("Equipment Objects (For Reset)")]
    public GameObject helmetObj;
    public GameObject gogglesObj;
    public GameObject glovesObj;

    [Header("UI Checklist")]
    public Text helmetUI;
    public Text gogglesUI;
    public Text glovesUI;
    public Text overallStatusUI;

    [Header("Feedback (Particles & Audio)")]
    public ParticleSystem completionParticle;
    public AudioSource successAudio;
    
    [Header("Progression Barrier")]
    public GameObject factoryDoor; // The door that blocks progress

    // Store initial transforms for the reset method
    private Vector3[] startPositions = new Vector3[3];
    private Quaternion[] startRotations = new Quaternion[3];

    void Start()
    {
        // 1. Save original positions/rotations for the Reset method
        SaveInitialTransforms();

        // 2. Subscribe to socket events to detect when items are equipped/unequipped
        helmetSocket.selectEntered.AddListener(OnItemEquipped);
        gogglesSocket.selectEntered.AddListener(OnItemEquipped);
        glovesSocket.selectEntered.AddListener(OnItemEquipped);

        helmetSocket.selectExited.AddListener(OnItemUnequipped);
        gogglesSocket.selectExited.AddListener(OnItemUnequipped);
        glovesSocket.selectExited.AddListener(OnItemUnequipped);

        UpdateUI();
    }

    private void SaveInitialTransforms()
    {
        startPositions[0] = helmetObj.transform.position;
        startRotations[0] = helmetObj.transform.rotation;
        
        startPositions[1] = gogglesObj.transform.position;
        startRotations[1] = gogglesObj.transform.rotation;
        
        startPositions[2] = glovesObj.transform.position;
        startRotations[2] = glovesObj.transform.rotation;
    }

    private void OnItemEquipped(SelectEnterEventArgs args)
    {
        successAudio.Play(); // Play sound on correct socket interaction
        CheckAllEquipment();
    }

    private void OnItemUnequipped(SelectExitEventArgs args)
    {
        CheckAllEquipment();
    }

    private void CheckAllEquipment()
    {
        bool hasHelmet = helmetSocket.hasSelection;
        bool hasGoggles = gogglesSocket.hasSelection;
        bool hasGloves = glovesSocket.hasSelection;

        // Update UI checklist
        helmetUI.text = hasHelmet ? "Helmet: Equipped (Ready)" : "Helmet: Pending";
        helmetUI.color = hasHelmet ? Color.green : Color.red;

        gogglesUI.text = hasGoggles ? "Goggles: Equipped (Ready)" : "Goggles: Pending";
        gogglesUI.color = hasGoggles ? Color.green : Color.red;

        glovesUI.text = hasGloves ? "Gloves: Equipped (Ready)" : "Gloves: Pending";
        glovesUI.color = hasGloves ? Color.green : Color.red;

        // Check if ALL are equipped to proceed
        if (hasHelmet && hasGoggles && hasGloves)
        {
            overallStatusUI.text = "PPE Complete! Proceed to Task.";
            overallStatusUI.color = Color.green;
            
            completionParticle.Play(); // Trigger particle effect upon success
            factoryDoor.SetActive(false); // Open door/allow player to proceed
        }
        else
        {
            overallStatusUI.text = "Please equip all PPE.";
            overallStatusUI.color = Color.yellow;
            factoryDoor.SetActive(true); // Keep door closed
        }
    }

    // Call this method from your Overall Scene/Stage Reset Button
    public void ResetStation()
    {
        // 1. Force items out of the sockets if they are currently attached
        ForceRemoveFromSocket(helmetSocket, helmetObj);
        ForceRemoveFromSocket(gogglesSocket, gogglesObj);
        ForceRemoveFromSocket(glovesSocket, glovesObj);

        // 2. Reset transforms to their initial state without reloading the scene
        ResetTransform(helmetObj, 0);
        ResetTransform(gogglesObj, 1);
        ResetTransform(glovesObj, 2);

        // 3. Reset Physics velocities so items don't fly away
        ResetPhysics(helmetObj);
        ResetPhysics(gogglesObj);
        ResetPhysics(glovesObj);

        // 4. Update UI and lock the barrier again
        CheckAllEquipment();
    }

    private void ForceRemoveFromSocket(XRSocketInteractor socket, GameObject obj)
    {
        if (socket.hasSelection && socket.interactablesSelected[0].transform.gameObject == obj)
        {
            XRBaseInteractable interactable = obj.GetComponent<XRBaseInteractable>();
            interactionManager.CancelInteractorSelection(socket); // Requires XR Interaction Manager reference in complex setups, or simply disabling/enabling socket
            
            // Alternative simple hack to force drop:
            socket.enabled = false;
            socket.enabled = true;
        }
    }

    private void ResetTransform(GameObject obj, int index)
    {
        obj.transform.position = startPositions[index];
        obj.transform.rotation = startRotations[index];
    }

    private void ResetPhysics(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}