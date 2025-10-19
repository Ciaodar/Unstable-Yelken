using UnityEngine;

// Basit düşman can scripti. Hasar alır, canı biterse ölür.
public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // 1. PUAN EKLE
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScoreForKill();
        }

        // 2. CANLI DÜŞMAN SAYISINI AZALT
        if (WaveSpawner.instance != null)
        {
            WaveSpawner.instance.EnemiesAlive--; 
        }

        // 3. Düşmanı yok et
        Destroy(gameObject);
    }
}