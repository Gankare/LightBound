using System.Collections;
using UnityEngine;

public class DoubleBarrelShotgun : MonoBehaviour
{
    [Header("References")]
    public Transform cameraHolder;  // 🔧 New: a pivot for camera recoil
    public Camera fpsCamera;
    public Transform leftMuzzle;
    public Transform rightMuzzle;
    public ParticleSystem muzzleFlashLeft;
    public ParticleSystem muzzleFlashRight;
    public GameObject impactPrefab;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public Animator animator;
    public FirstPersonController controller;

    [Header("Shotgun Settings")]
    public int magazineSize = 2;
    public bool unlimitedReserve = true;
    public int reserveAmmo = 16;
    public float reloadTime = 1.4f;
    public float fireRate = 0.5f;
    public float range = 50f;
    public float damagePerPellet = 6f;
    public int pelletsPerShot = 8;
    public float spreadAngle = 10f;

    [Header("Recoil")]
    public float recoilAmount = 3f;
    public float recoilRecoverSpeed = 6f;

    private int currentAmmo;
    private bool isReloading = false;
    private float lastFireTime = -10f;
    public AudioSource audioSource;
    private int nextBarrel = 0;
    private float currentRecoil = 0f;

    void Awake()
    {
        if (fpsCamera == null && Camera.main != null) fpsCamera = Camera.main;
        if (cameraHolder == null && fpsCamera != null) cameraHolder = fpsCamera.transform; // fallback
    }

    void Start()
    {
        currentAmmo = magazineSize;
    }

    void Update()
    {
        if (isReloading) return;

        if ((Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0)) && Time.time - lastFireTime >= fireRate)
        {
            TryFire();
        }

        // Recoil recovery
        if (currentRecoil > 0f && cameraHolder != null)
        {
            float recover = recoilRecoverSpeed * Time.deltaTime;
            float step = Mathf.Min(recover, currentRecoil);
            currentRecoil -= step;

            // recover only local pitch
            var localRot = cameraHolder.localEulerAngles;
            float pitch = NormalizeAngle(localRot.x);
            pitch -= step;
            cameraHolder.localRotation = Quaternion.Euler(pitch, localRot.y, localRot.z);
        }

        // Manual reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < magazineSize && (unlimitedReserve || reserveAmmo > 0))
                StartCoroutine(Reload());
        }
    }

    private void TryFire()
    {
        if (Time.time - lastFireTime < fireRate) return;
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            if (unlimitedReserve || reserveAmmo > 0)
                StartCoroutine(Reload());
            return;
        }

        if (nextBarrel == 0) FireBarrel(leftMuzzle, muzzleFlashLeft);
        else FireBarrel(rightMuzzle, muzzleFlashRight);

        nextBarrel = 1 - nextBarrel;
        lastFireTime = Time.time;
        currentAmmo--;

        if (currentAmmo <= 0 && (unlimitedReserve || reserveAmmo > 0))
            StartCoroutine(Reload());
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
                var health = hit.collider.GetComponent<Health>();
                if (health != null) health.TakeDamage(damagePerPellet);

                if (hit.rigidbody != null)
                    hit.rigidbody.AddForceAtPosition(dir * 50f, hit.point, ForceMode.Impulse);

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
        if (controller != null)
        {
            controller.AddRecoil(recoilAmount);
        }
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;

        if (animator != null) animator.SetTrigger("Reload");
        if (reloadSound != null)
        {
            yield return new WaitForSeconds(0.8f);
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

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

    private float NormalizeAngle(float a)
    {
        while (a > 180f) a -= 360f;
        while (a < -180f) a += 360f;
        return a;
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetReserveAmmo() => unlimitedReserve ? int.MaxValue : reserveAmmo;
}
