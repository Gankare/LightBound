using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 1;
    private bool dead = false;

    public void TakeDamage(float damagePerPellet)
    {
        health -= damagePerPellet;
        CheckHealth();
    }

    public void CheckHealth()
    {
        if (health < 0 && !dead)
        {
            Death();
        }

    }
    private void Death()
    {
        dead = true;
        //Remove movement and dissolveä
    }
}
