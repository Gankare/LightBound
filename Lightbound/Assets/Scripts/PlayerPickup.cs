using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup settings")]
    public KeyCode pickupKey = KeyCode.E;
    public Transform weaponMount; 
    public Vector3 mountLocalPosition = Vector3.zero; 
    public Vector3 mountLocalEuler = Vector3.zero;

    // keep reference to current nearby pickup
    private WeaponPickup nearbyWeapon;

    void Update()
    {
        // Find a nearby weapon (we poll every frame by scanning collisions, or we could be notified)
        // Here we use a simple search for WeaponPickup in triggers—if you prefer event-driven, see note below.
        FindNearbyWeapon();

        if (nearbyWeapon != null)
        {
            // Optional: display UI prompt from player side if you want centralized UI
            if (Input.GetKeyDown(pickupKey))
            {
                PickupWeapon(nearbyWeapon);
            }
        }
    }

    private void FindNearbyWeapon()
    {
        // If we already have a nearbyWeapon and it's still valid, keep it
        if (nearbyWeapon != null)
        {
            if (nearbyWeapon.playerInRange) return;
            nearbyWeapon = null;
        }

        // Otherwise find the closest WeaponPickup within a radius (safe fallback)
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f); // tweak radius
        float bestDist = float.MaxValue;
        foreach (var c in hits)
        {
            var wp = c.GetComponent<WeaponPickup>();
            if (wp != null && wp.playerInRange)
            {
                float d = Vector3.SqrMagnitude(transform.position - wp.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearbyWeapon = wp;
                }
            }
        }
    }

    private void PickupWeapon(WeaponPickup wp)
    {
        if (wp == null) return;

        // parent it to the weapon mount
        if (weaponMount == null)
        {
            Debug.LogWarning("PlayerPickup: weaponMount not assigned.");
            return;
        }

        // Optionally detach previous weapon (drop / destroy) - implement as you like
        // Example: if player already has child in mount, destroy it
        if (weaponMount.childCount > 0)
        {
            for (int i = 0; i < weaponMount.childCount; i++)
                Destroy(weaponMount.GetChild(i).gameObject);
        }

        // Calculate desired local rotation/position from inspector values
        Quaternion localRot = Quaternion.Euler(mountLocalEuler);
        Vector3 localPos = mountLocalPosition;

        wp.OnPickedUp(weaponMount, localPos, localRot);

        // Clear nearby reference
        nearbyWeapon = null;
    }

    // debug: visualize pickup radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
