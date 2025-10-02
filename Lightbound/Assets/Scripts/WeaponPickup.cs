using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup settings")]
    public string weaponName = "Shotgun";
    public Transform equipPosition; // optional local position/rotation override (child of weapon)
    public bool equipOnPickup = true;

    [Header("Optional visual")]
    public GameObject promptUI; // small "Press E to pick up" object (can be a world-space canvas or disabled UI)

    // runtime
    [HideInInspector] public bool playerInRange = false;
    [HideInInspector] public Transform playerTransform;

    private Collider _collider;
    private Rigidbody _rb;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true; // pickup using trigger
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (promptUI) promptUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
            if (promptUI) promptUI.SetActive(true);
            // optionally, notify player script - not required if player polls
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
            if (promptUI) promptUI.SetActive(false);
        }
    }

    /// <summary>
    /// Perform the pickup logic: parent to hand, disable physics, return the GameObject.
    /// Player script should call this.
    /// </summary>
    public void OnPickedUp(Transform parent, Vector3 localPos, Quaternion localRot)
    {
        // Hide prompt
        if (promptUI) promptUI.SetActive(false);

        // Disable physics
        if (_rb) { _rb.isKinematic = true; _rb.detectCollisions = false; }
        if (_collider) _collider.enabled = false;

        // Reparent and set local transform
        transform.SetParent(parent, worldPositionStays: false);

        if (equipPosition != null)
        {
            // If weapon prefab contains an equipPosition marker, use it
            transform.localPosition = -equipPosition.localPosition; // adjust if necessary
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            transform.localPosition = localPos;
            transform.localRotation = localRot;
        }

        // Optionally enable weapon scripts (if you want weapon disabled until picked)
        var weaponBehaviour = GetComponent<MonoBehaviour>(); // replace with your weapon script type if needed
        if (weaponBehaviour != null && !weaponBehaviour.enabled)
            weaponBehaviour.enabled = true;
    }
}
