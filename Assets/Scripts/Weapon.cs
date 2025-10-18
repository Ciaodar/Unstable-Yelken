using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Şarjör Ayarları")]
    public int maxAmmo = 50; // Şarjörün maksimum alabildiği mermi (güncellendi)
    public int currentAmmo; // Anlık mermi sayısı

    [Header("Ateşleme Ayarları")]
    [Tooltip("Saniyedeki atış sayısı (fireRate=5 -> her atış 0.2s)")]
    public float fireRate = 5f;
    public float damagePerShot = 20f;

    [Header("Unstable Mekaniği")]
    [Tooltip("Şarjör tamamen boşken hassasiyet ne kadar artsın? 1 = Değişmez, 2 = 2 katına çıkar.")]
    [Range(1f, 10f)]
    public float maxSensitivityMultiplier = 2.5f; // Şarjör boşken hassasiyet bu kadarla çarpılacak.

    [Header("Aşırı Isınma")]
    [Tooltip("Kesintisiz ateşleme süresi (saniye) -> bu süreden sonra oyuncuya hasar vermeye başlar")]
    public float overheatThreshold = 3f;
    [Tooltip("Aşırı ısınma sırasında oyuncuya saniyede verilen hasar")]
    public float overheatDamagePerSecond = 10f;

    [Header("Görsel Referanslar")]
    public Transform muzzleTransform; // Silahın ucu (lazer başlangıç noktası)
    public LineRenderer laserLine; // Lazer görseli (LineRenderer)
    [Tooltip("Raycast'in etkileyeceği katmanlar")] 
    public LayerMask hitLayers = ~0; // Varsayılan: her şeyi hedefle
    [Tooltip("Silahın rengini değiştirecek renderer'lar")]
    public Renderer[] weaponRenderers;
    [Tooltip("Isınma renginin dolum ölçeği (1 = overheatThreshold'ta tam geçiş)" )]
    public float heatColorScale = 1.5f;
    [Tooltip("Mermi tüpündeki sıvıyı temsil eden transform. Pivot alt tarafta olmalı ve Y ölçeği ile doluluk gösterilecek.")]
    public Transform ammoLiquid;

    // Özel durum değişkenleri
    private float fireCooldown = 0f;
    private float continuousFireTime = 0f;
    private bool isOverheated = false;

    private PlayerHealth playerHealth;
    private Camera mainCamera;

    void Start()
    {
        // Oyuna dolu şarjörle başla
        Reload();

        // Kamera referansını önbelleğe al
        mainCamera = Camera.main;

         // Eğer LineRenderer sahneye eklenmemişse bileşen eklemeye çalış
         if (laserLine == null)
         {
             laserLine = GetComponent<LineRenderer>();
             if (laserLine == null)
             {
                 laserLine = gameObject.AddComponent<LineRenderer>();
                 // Basit görsel ayarlar (daha sonra düzenleyin)
                 laserLine.startWidth = 0.02f;
                 laserLine.endWidth = 0.02f;
                 laserLine.positionCount = 2;
                laserLine.useWorldSpace = true;
             }
         }

        // PlayerHealth referansını bul (oyuncu GameObject'inin "Player" tag'ine sahip olduğunu varsayar)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        // Başlangıçta lazer kapalı olsun
        if (laserLine != null) laserLine.enabled = false;
    }

    void Update()
    {
        // Sürekli basılı tutma ile ateşleme
        bool isHoldingFire = Input.GetButton("Fire1");

        // Lazer görselini her frame güncelle (eğer tutuyorsak ve lazer açık)
        if (isHoldingFire && laserLine != null)
        {
            UpdateLaserVisual();
        }

        // Aşırı ısınma zararını uygula (sadece threshold aşıldıktan sonra) -- continuousFireTime ile doğru orantılı
        if (isHoldingFire && continuousFireTime > overheatThreshold)
        {
            if (playerHealth != null)
            {
                float scale = continuousFireTime / overheatThreshold; // 1 = threshold, >1 daha fazla
                float damageThisFrame = overheatDamagePerSecond * scale * Time.deltaTime;
                playerHealth.TakeDamage(damageThisFrame);
            }
        }

        // Ateşleme mantığı: artık overheat olsa bile ateş etmeye izin veriyoruz
        if (isHoldingFire && currentAmmo > 0)
        {
            // Lazer görünür hale getir
            if (laserLine != null) laserLine.enabled = true;

            // Sürekli ateş zamanını arttır (açık ateş bittikten sonra soğuyacak)
            continuousFireTime += Time.deltaTime;

            // Ateş hızı kontrolü
            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }

            // Aşırı ısınma flag'i sadece bilgi amaçlı
            if (continuousFireTime > overheatThreshold)
            {
                isOverheated = true;
            }
        }
        else
        {
            // Lazer kapat
            if (laserLine != null) laserLine.enabled = false;

            // Tutma durduğunda sürekli ateş zamanını düşür (soğuma)
            continuousFireTime = Mathf.Max(0f, continuousFireTime - Time.deltaTime);

            // Soğuduktan sonra aşırı ısınma durumunu kaldır
            if (continuousFireTime < overheatThreshold)
            {
                isOverheated = false;
            }
        }

        // Manual reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

        // Isınma ve mermi tüpü görsellerini güncelle
        UpdateWeaponHeatVisual();
        UpdateAmmoVisual();
    }

    void Shoot()
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;
        Debug.Log("Ateş edildi! Kalan mermi: " + currentAmmo);

        // Ekranın ortasından bir ray gönder
        Camera cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main Camera bulunamadı. Raycast çalışmayacak.");
            return;
        }

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;
        Vector3 endPoint = ray.origin + ray.direction * 100f; // Eğer hiçbir şeye çarpmazsa 100 birim ileri

        if (Physics.Raycast(ray, out hit, 100f, hitLayers))
        {
            endPoint = hit.point;

            // Eğer çarpılan objede EnemyHealth varsa hasar ver
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damagePerShot);
            }
        }

        // Lazer görselini güncelle
        if (laserLine != null)
        {
            Vector3 startPos = (muzzleTransform != null) ? muzzleTransform.position : transform.position;
            laserLine.SetPosition(0, startPos);
            laserLine.SetPosition(1, endPoint);
        }

        // Ateş sonrası görsel güncelle (anlık)
        UpdateAmmoVisual();
    }

    // Silahın ısınma düzeyine göre renderer'ların rengini değiştirir.
    void UpdateWeaponHeatVisual()
    {
        if (weaponRenderers == null || weaponRenderers.Length == 0) return;

        // Normalize edilmiş ısı değeri
        float t = Mathf.Clamp01(continuousFireTime / (overheatThreshold * heatColorScale));

        // Renk geçişi: mavi -> sarı -> kırmızı -> kahverengi
        Color color;
        if (t < 0.33f)
        {
            float s = t / 0.33f;
            color = Color.Lerp(new Color(0f, 0.5f, 1f), new Color(1f, 0.95f, 0f), s); // mavi -> sarı
        }
        else if (t < 0.66f)
        {
            float s = (t - 0.33f) / 0.33f;
            color = Color.Lerp(new Color(1f, 0.95f, 0f), Color.red, s); // sarı -> kırmızı
        }
        else
        {
            float s = (t - 0.66f) / 0.34f;
            color = Color.Lerp(Color.red, new Color(0.4f, 0.2f, 0f), s); // kırmızı -> kahverengi
        }

        foreach (var r in weaponRenderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                // URP'de materyaller _BaseColor kullanabilir, eski shader'lar _Color kullanır.
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            }
        }
    }

    // Ekranın ortasından ray atıp LineRenderer'ı günceller.
    void UpdateLaserVisual()
    {
        if (mainCamera == null || laserLine == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;
        Vector3 endPoint = ray.origin + ray.direction * 100f;
        if (Physics.Raycast(ray, out hit, 100f, hitLayers))
        {
            endPoint = hit.point;
        }

        Vector3 startPos = (muzzleTransform != null) ? muzzleTransform.position : transform.position;
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, endPoint);
    }

    // Mermi tüpündeki sıvı seviyesini güncelle (ammoLiquid varsayılan pivot alt tarafta olmalı)
    void UpdateAmmoVisual()
    {
        if (ammoLiquid == null) return;
        float fill = (maxAmmo > 0) ? (float)currentAmmo / maxAmmo : 0f;
        Vector3 localScale = ammoLiquid.localScale;
        localScale.y = Mathf.Clamp01(fill);
        ammoLiquid.localScale = localScale;
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
