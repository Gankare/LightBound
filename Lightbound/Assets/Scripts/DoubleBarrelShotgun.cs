using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoubleBarrelShotgun : MonoBehaviour
{
    [Header("References")]
    public Camera fpsCamera;
    public Transform leftMuzzle;
    public Transform rightMuzzle;
    public ParticleSystem muzzleFlashLeft;
    public ParticleSystem muzzleFlashRight;
    public GameObject impactPrefab;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public Animator animator;

    [Header("Shotgun Settings")]
    public int magazineSize = 2;            // 2 shells (double barrel)
    public bool unlimitedReserve = true;    // if true, reserve ammo is infinite
    public int reserveAmmo = 16;            // only used when unlimitedReserve == false
    public float reloadTime = 1.4f;         // time to reload both shells
    public float fireRate = 0.5f;           // seconds between individual barrel shots
    public float range = 50f;
    public float damagePerPellet = 6f;
    public int pelletsPerShot = 8;          // pellets per barrel shot
    public float spreadAngle = 10f;         // spread cone degrees (total)

    [Header("Recoil")]
    public float recoilAmount = 3f;
    public float recoilRecoverSpeed = 6f;

    // runtime
    private int currentAmmo;                // shells in magazine
    private bool isReloading = false;
    private float lastFireTime = -10f;
    private AudioSource audioSource;
    private int nextBarrel = 0;             // 0 = left, 1 = right
    private float currentRecoil = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (fpsCamera == null && Camera.main != null) fpsCamera = Camera.main;
    }

    void Start()
    {
        currentAmmo = magazineSize;
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0) && Time.time - lastFireTime >= fireRate && false)
        {
            Debug.Log("Fire");
            TryFire();
        }

        // Manual reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < magazineSize && (unlimitedReserve || reserveAmmo > 0))
                StartCoroutine(Reload());
        }

        // Recoil recovery
        if (currentRecoil > 0f)
        {
            float recover = recoilRecoverSpeed * Time.deltaTime;
            float step = Mathf.Min(recover, currentRecoil);
            currentRecoil -= step;

            if (fpsCamera != null)
            {
                var camLocal = fpsCamera.transform.localEulerAngles;
                float pitch = NormalizeAngle(camLocal.x);
                pitch -= step;
                fpsCamera.transform.localEulerAngles = new Vector3(pitch, camLocal.y, camLocal.z);
            }
        }
    }

    private void TryFire()
    {
        if (Time.time - lastFireTime < fireRate) return; // rate limit
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            // no shells: auto reload if reserve available
            if (unlimitedReserve || reserveAmmo > 0)
            {
                StartCoroutine(Reload());
            }
            else
            {
                // optionally play empty sound
            }
            return;
        }

        // Fire selected barrel
        if (nextBarrel == 0) FireBarrel(leftMuzzle, muzzleFlashLeft);
        else FireBarrel(rightMuzzle, muzzleFlashRight);

        // alternate barrel for next shot
        nextBarrel = 1 - nextBarrel;
        lastFireTime = Time.time;
        currentAmmo--;

        // If both barrels spent, auto reload if possible
        if (currentAmmo <= 0 && (unlimitedReserve || reserveAmmo > 0))
        {
            StartCoroutine(Reload());
        }
    }

    private void FireBarrel(Transform muzzle, ParticleSystem muzzleFlash)
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        if (fireSound != null) audioSource.PlayOneShot(fireSound);
        if (animator != null) animator.SetTrigger("Shoot");

        ApplyRecoil();

        // Fire pellets (hitscan)
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 dir = GetSpreadDirection(muzzle.forward, spreadAngle);
            if (Physics.Raycast(muzzle.position, dir, out RaycastHit hit, range))
            {
                // Apply damage if target has health
                var health = hit.collider.GetComponent<Health>(); // adapt to your Health script
                if (health != null)
                {
                    health.TakeDamage(damagePerPellet);
                }

                // apply physics impulse
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForceAtPosition(dir * 50f, hit.point, ForceMode.Impulse);
                }

                // impact VFX
                if (impactPrefab != null)
                {
                    var fx = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx, 4f);
                }
            }
        }
    }

    private Vector3 GetSpreadDirection(Vector3 forward, float angle)
    {
        if (angle <= 0f) return forward;
        float half = angle * 0.5f;
        float yaw = Random.Range(-half, half);
        float pitch = Random.Range(-half, half);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        return rot * forward;
    }

    private void ApplyRecoil()
    {
        if (fpsCamera == null) return;
        var camLocal = fpsCamera.transform.localEulerAngles;
        float pitch = NormalizeAngle(camLocal.x);
        pitch += recoilAmount;
        fpsCamera.transform.localEulerAngles = new Vector3(pitch, camLocal.y, camLocal.z);
        currentRecoil += recoilAmount;
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;

        if (animator != null) animator.SetTrigger("Reload");
        if (reloadSound != null) audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        // refill magazine
        if (unlimitedReserve)
        {
            currentAmmo = magazineSize;
        }
        else
        {
            int need = magazineSize - currentAmmo;
            int taken = Mathf.Min(need, reserveAmmo);
            currentAmmo += taken;
            reserveAmmo -= taken;
        }

        isReloading = false;
    }

    // helper to normalize angles from 0..360 to -180..180
    private float NormalizeAngle(float a)
    {
        while (a > 180f) a -= 360f;
        while (a < -180f) a += 360f;
        return a;
    }

    // Public helpers
    public int GetCurrentAmmo() => currentAmmo;
    public int GetReserveAmmo() => unlimitedReserve ? int.MaxValue : reserveAmmo;
}
