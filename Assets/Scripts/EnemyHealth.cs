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
        Debug.Log(gameObject.name + " hasar aldı: " + amount + " kalan: " + currentHealth);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Ölme efekti/animasyon eklemek istersen burada yap.
        Destroy(gameObject);
    }
}

