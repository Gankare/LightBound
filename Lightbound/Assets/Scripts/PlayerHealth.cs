using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Image damagedImageRed;
    public Image damagedImageBlack;
    private int currentHealth;
    public float regenRate = 2f;          
    public float regenDelay = 5f;         

    private float lastDamageTime;         
    private Coroutine regenCoroutine;
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthVisuals();
        regenCoroutine = StartCoroutine(RegenTimer());
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        lastDamageTime = Time.time;          
        UpdateHealthVisuals();
        if (currentHealth <= 0) Die();
    }
    private IEnumerator RegenTimer()
    {
        while (true)                     
        {
            yield return new WaitWhile(() => Time.time - lastDamageTime < regenDelay);
            while (currentHealth < maxHealth)
            {
                yield return new WaitForSeconds(1f / regenRate);
                if (Time.time - lastDamageTime < regenDelay) break; 

                currentHealth++;
                UpdateHealthVisuals();
            }
        }
    }
    void Die()
    {
        Debug.Log("Player Died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    void UpdateHealthVisuals()
    {
        if (damagedImageBlack != null)
        {
            if (currentHealth < maxHealth)
            {
                float alphaValueRed = 1f - (float)currentHealth / maxHealth;
                float alphaValueBlack = 0.85f - (float)currentHealth / maxHealth;
                damagedImageRed.color = new Color(damagedImageRed.color.r,
                                               damagedImageRed.color.g,
                                               damagedImageRed.color.b,
                                               alphaValueRed);
                damagedImageBlack.color = new Color(damagedImageBlack.color.r,
                                               damagedImageBlack.color.g,
                                               damagedImageBlack.color.b,
                                               alphaValueBlack);
            }
            else
            {
                float alphaValue = 0;
                damagedImageRed.color = new Color(damagedImageRed.color.r,
                                               damagedImageRed.color.g,
                                               damagedImageRed.color.b,
                                               alphaValue);
                damagedImageBlack.color = new Color(damagedImageBlack.color.r,
                                               damagedImageBlack.color.g,
                                               damagedImageBlack.color.b,
                                               alphaValue);
            }
        }
    }
}