using UnityEngine;
using System;

// Oyuncu can scripti. Basitçe hasar alır ve canı bitince oyunu etkiler.
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead;
    [SerializeField] private ZombiliedScreen deathScreen;
    
    // Diğer scriptlerin oyuncunun mevcut canını okuyabilmesi için public readonly property
    public float CurrentHealth { get { return currentHealth; } }
    

    [Header("Rejenere Ayarları")]
    [Tooltip("Hasar aldıktan sonra kaç saniye içinde yenilenmeye başlamaz")] public float regenDelay = 2f;
    [Tooltip("Saniyede yenilenen can miktarı")] public float regenRate = 25f;

    private float _lastDamageTime = -999f;

    // Hasar alındığında yayınlanan olay (miktar)
    public event Action<float> OnTakeDamage;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Eğer yeterli süre geçtiyse hızlıca can yenile
        if (currentHealth < maxHealth && Time.time - _lastDamageTime > regenDelay)
        {
            currentHealth += regenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        _lastDamageTime = Time.time;
        currentHealth -= amount;
        Debug.Log("Oyuncu hasar aldı: " + amount + " kalan: " + currentHealth);

        // Olayı yayınla
        OnTakeDamage?.Invoke(amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return; // Zaten öldüyse tekrar ölme
        isDead = true;
        Debug.Log("Oyuncu öldü.");
        // Basit davranış: oyuncuyu pasif hale getir
        deathScreen.Show();
        gameObject.SetActive(false);
        // İsterseniz buraya bir yeniden başlatma veya sahne yükleme ekleyin.
    }
}
