using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 1f;
    private bool dead = false;

    [Header("Death FX")]
    [Tooltip("Optional death effect (particle prefab).")]
    public GameObject deathEffect;
    public GameObject deathSound;

    public void TakeDamage(float damagePerPellet)
    {
        if (dead) return;

        health -= damagePerPellet;
        CheckHealth();
    }

    private void CheckHealth()
    {
        if (health <= 0 && !dead)
        {
            Death();
        }
    }

    private void Death()
    {
        dead = true;

        if (deathEffect != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1f;
            Quaternion uprightRotation = Quaternion.LookRotation(Vector3.up); // face upward
            GameObject fx = Instantiate(deathEffect, spawnPos, uprightRotation);
            GameObject sx = Instantiate(deathSound, spawnPos, uprightRotation);
            Destroy(sx, 1f);
        }
        Destroy(gameObject);
    }
}
