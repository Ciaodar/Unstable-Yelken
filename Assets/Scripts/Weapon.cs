using UnityEngine;

public class Weapon : MonoBehaviour
{
  [Header("Şarjör Ayarları")]
    public int maxAmmo = 30; // Şarjörün maksimum alabildiği mermi
    public int currentAmmo; // Anlık mermi sayısı

    [Header("Unstable Mekaniği")]
    [Tooltip("Şarjör tamamen boşken hassasiyet ne kadar artsın? 1 = Değişmez, 2 = 2 katına çıkar.")]
    [Range(1f, 10f)]
    public float maxSensitivityMultiplier = 2.5f; // Şarjör boşken hassasiyet bu kadarla çarpılacak.

    void Start()
    {
        // Oyuna dolu şarjörle başla
        Reload();
    }

    void Update()
    {
        // Sol tıka basınca ateş et (basit bir test için)
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        // R tuşuna basınca şarjör doldur
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    void Shoot()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            Debug.Log("Ateş edildi! Kalan mermi: " + currentAmmo);
        }
        else
        {
            Debug.Log("Mermi bitti! Şarjör değiştir!");
        }
    }

    void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Şarjör dolduruldu! Mermi: " + currentAmmo);
    }

    // --- MEKANİĞİN EN ÖNEMLİ FONKSİYONU ---
    // Bu fonksiyon, mermi sayısına göre 1 ile maxSensitivityMultiplier arasında bir değer döndürür.
    public float GetSensitivityMultiplier()
    {
        // Merminin yüzde kaçının kaldığını hesapla (0.0 ile 1.0 arasında)
        // Örneğin, 30/30 mermi varsa 1.0, 15/30 mermi varsa 0.5, 0/30 mermi varsa 0.0 olur.
        float ammoPercentage = (float)currentAmmo / maxAmmo;

        // Lerp fonksiyonu ile hassasiyet çarpanını hesapla.
        // Mermi %100 (dolu) iken çarpan 1 olsun (normal hassasiyet).
        // Mermi %0 (boş) iken çarpan maxSensitivityMultiplier olsun (en yüksek hassasiyet).
        // Matematik.Lerp(A, B, t) -> t=0 ise A'yı, t=1 ise B'yi verir.
        // Bizim durumumuzda tam tersi lazım, mermi azaldıkça hassasiyet artacak.
        // Bu yüzden A yerine max değeri, B yerine min değeri yazıyoruz.
        return Mathf.Lerp(maxSensitivityMultiplier, 1f, ammoPercentage);
    }
}
