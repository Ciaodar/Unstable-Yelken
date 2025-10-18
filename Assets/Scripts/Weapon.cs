using UnityEngine;

public class Weapon : MonoBehaviour
{
    // === YENİ SES AYARLARI ===
    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public AudioClip shootClip; // Ateş etme sesi
    public AudioClip reloadStartClip; // Reload'a başlama sesi
    public AudioClip reloadFinishClip; // Reload'u bitirme sesi (isteğe bağlı)
    // ========================
    
    [Header("Şarjör Ayarları")]
    public int maxAmmo = 50; // Şarjörün maksimum alabildiği mermi (güncellendi)
    public int currentAmmo;
    float timeToAddAmmo = 0;// Anlık mermi sayısı

    [Header("Ateşleme Ayarları")]
    [Tooltip("Saniyedeki atış sayısı (fireRate=5 -> her atış 0.2s)")]
    public float fireRate = 5f;
    public float damagePerShot = 10f;

    [Header("Unstable Mekaniği")]
    [Tooltip("Şarjör tamamen boşken hassasiyet ne kadar artsın? 1 = Değişmez, 2 = 2 katına çıkar.")]
    [Range(1f, 50f)]
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
    [Tooltip("Şarjör BlendShape'lerini içeren SkinnedMeshRenderer")]
    public SkinnedMeshRenderer ammoBlendShapeRenderer;
    [Tooltip("BlendShape animasyonunun yumuşatma hızı")]
    public float blendShapeSmoothSpeed = 5f;

    [Header("Reload Ayarları")]
    [Tooltip("Saniyede doldurulan mermi sayısı")]
    public float reloadAmmoPerSecond = 10f;
    [Tooltip("Reload sırasında silahın ineceği Y pozisyonu (local)")]
    public float reloadLowerYPosition = -0.6f;
    [Tooltip("Silahın yukarı/aşağı hareket hızı")]
    public float weaponLowerSpeed = 8f;
    
    // Özel durum değişkenleri
    private float fireCooldown = 0f;
    private float continuousFireTime = 0f;
    private bool isOverheated = false;

    private PlayerHealth playerHealth;
    private Camera mainCamera;
    
    // BlendShape hedef ve mevcut değerler
    private float _targetBittiBlend = 0f;
    private float _currentBittiBlend = 0f;
    private float _targetKey2Blend = 0f;
    private float _currentKey2Blend = 0f;

    // Reload durumu
    private bool _isReloading = false;
    private Vector3 _weaponOriginalLocalPosition;
    private float _targetWeaponYPosition;
    private float _currentWeaponYPosition;

    void Start()
    {
        // Oyuna dolu şarjörle başla
        currentAmmo = maxAmmo;
        CalculateAmmoBlendShapeTargets();

        // Kamera referansını önbelleğe al
        mainCamera = Camera.main;

        // Silahın başlangıç pozisyonunu kaydet
        _weaponOriginalLocalPosition = transform.localPosition;
        _currentWeaponYPosition = _weaponOriginalLocalPosition.y;
        _targetWeaponYPosition = _weaponOriginalLocalPosition.y;

        // === SES KAYNAĞINI BULMA ===
        if (audioSource == null)
        {
            // Bu script'in üzerinde AudioSource arar
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Bulamazsa yeni bir bileşen ekler
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        // ==========================
        if (laserLine != null) laserLine.enabled = false;
        
        
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
        bool isHoldingReload = Input.GetKey(KeyCode.R);

        // Reload mekanizması
        if (isHoldingReload && currentAmmo < maxAmmo)
        {
            if (!_isReloading)
            {
                // Reload başladı
                _isReloading = true;
                _targetWeaponYPosition = _weaponOriginalLocalPosition.y + reloadLowerYPosition;
                
                // === RELOAD BAŞLANGIÇ SESİ: Önceki sesi durdur ve yeni sesi çal ===
                if (audioSource != null && reloadStartClip != null)
                {
                    // Reload başladığında önceki sesi kes ve reload sesini çal
                    audioSource.Stop();
                    audioSource.clip = reloadStartClip;
                    audioSource.loop = true; // Reload süresince ses çalsın
                    audioSource.Play();
                }
                // =================================================================
            }
            
            // 1/reloadAmmoPerSecond süre sonra 1 mermi doldur
            if (currentAmmo < maxAmmo)
            {
                if (timeToAddAmmo < 1 / reloadAmmoPerSecond)
                    timeToAddAmmo += Time.deltaTime;
                else
                {
                    currentAmmo += 1;
                    timeToAddAmmo = 0f; 
                }
            }
            CalculateAmmoBlendShapeTargets();
        }
        else // Reload tuşu bırakıldı VEYA mermi doldu
        {
            if (_isReloading)
            {
                // Reload bitti, silahı kaldır
                _isReloading = false;
                _targetWeaponYPosition = _weaponOriginalLocalPosition.y;

                // === RELOAD BİTİŞ SESİ: Reload sesini durdur, bitiş sesini çal ===
                if (audioSource != null)
                {
                    audioSource.Stop(); // Devam eden reload sesini kes
                    if (reloadFinishClip != null)
                    {
                        audioSource.PlayOneShot(reloadFinishClip);
                    }
                }
                // ===============================================================
            }
        }
        
        // Silahın yukarı/aşağı animasyonunu güncelle
        _currentWeaponYPosition = Mathf.Lerp(_currentWeaponYPosition, _targetWeaponYPosition, Time.deltaTime * weaponLowerSpeed);
        Vector3 newPos = transform.localPosition;
        newPos.y = _currentWeaponYPosition;
        transform.localPosition = newPos;

        // Reload sırasında silahı kullanamayız
        if (!_isReloading)
        {
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
                    // Overheat hasarı continuousFireTime ile üstel olarak artar
                    float overheatRatio = (continuousFireTime - overheatThreshold) / overheatThreshold;
                    float damageMultiplier = 1f + (overheatRatio * overheatRatio * 2f); // Üstel artış
                    float damageThisFrame = overheatDamagePerSecond * damageMultiplier * Time.deltaTime;
                    playerHealth.TakeDamage(damageThisFrame);
                }
            }

          // Ateşleme mantığı: artık overheat olsa bile ateş etmeye izin veriyoruz
            if (isHoldingFire && currentAmmo > 0)
            {
                // **ATEŞ SESİNİ BAŞLAT/DEVAM ETTİR**
                if (audioSource != null && shootClip != null)
                {
                    // Eğer reload sesi çalıyorsa onu durdur
                    if (audioSource.isPlaying && audioSource.clip == reloadStartClip)
                    {
                        audioSource.Stop();
                    }

                    // Ateş sesi çalmaya başlamadıysa başlat
                    if (audioSource.clip != shootClip || !audioSource.isPlaying)
                    {
                        audioSource.clip = shootClip;
                        audioSource.loop = true; // Sürekli çalması için loop açıldı
                        audioSource.Play();
                    }
                }
                
                // Lazer görünür hale getir
                if (laserLine != null) laserLine.enabled = true;

                // Sürekli ateş zamanını arttır (açık ateş bittikten sonra soğuyacak)
                continuousFireTime += Time.deltaTime;

                // Ateş hızı kontrolü
                fireCooldown -= Time.deltaTime;
                if (fireCooldown <= 0f)
                {
                    // Sadece mermi tüketimini ve raycast'i tetikle
                    Shoot(); 
                    fireCooldown = 1f / fireRate;
                }

                // Aşırı ısınma flag'i sadece bilgi amaçlı
                if (continuousFireTime > overheatThreshold)
                {
                    isOverheated = true;
                }
            }
            else // Ateş tuşu bırakıldı VEYA Mermi bitti
            {
                // **ATEŞ SESİNİ DURDUR**
                if (audioSource != null && audioSource.isPlaying && audioSource.clip == shootClip)
                {
                    audioSource.Stop();
                }

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
        }
        else
        {
            // Reload sırasında lazer kapalı
            if (laserLine != null) laserLine.enabled = false;
            
            // Reload sırasında ateş sesini durdur (önlem amaçlı)
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == shootClip)
            {
                audioSource.Stop();
            }
            
            // Soğuma devam etsin reload sırasında
            continuousFireTime = Mathf.Max(0f, continuousFireTime - Time.deltaTime);
            if (continuousFireTime < overheatThreshold)
            {
                isOverheated = false;
            }
        }

        // Isınma ve mermi tüpü görsellerini güncelle
        UpdateWeaponHeatVisual();
        UpdateAmmoBlendShapes();
        UpdateLaserColor();
    }

    void Shoot()
    {
        if (currentAmmo <= 0) 
        {
            // Mermi bittiyse sesi durdur (Update'de zaten yapılıyor ama burada da bir kontrol iyi olabilir)
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == shootClip)
            {
                audioSource.Stop();
            }
            return;
        }
        
        currentAmmo--;
        Debug.Log("Ateş edildi! Kalan mermi: " + currentAmmo);

        // === YENİ: ATEŞ ETME SESİ ===
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }
        // ==========================
        
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

        // Ateş sonrası BlendShape hedeflerini güncelle
        CalculateAmmoBlendShapeTargets();
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

    // BlendShape hedef değerlerini mermi miktarına göre hesapla
    void CalculateAmmoBlendShapeTargets()
    {
        if (maxAmmo <= 0) return;
        
        float fillPercentage = (float)currentAmmo / maxAmmo * 100f; // 0-100 arası
        
        // Faz 1: %100'den %15'e kadar -> "bitti" 0'dan 100'e
        // Faz 2: %15'ten %0'a kadar -> "bitti" 40'a, "Key 2" 80'e
        
        if (fillPercentage >= 15f)
        {
            // Faz 1: 100% -> 15%
            // bitti: 0 -> 100
            float phase1Progress = Mathf.InverseLerp(100f, 15f, fillPercentage); // 0 to 1
            _targetBittiBlend = Mathf.Lerp(0f, 100f, phase1Progress);
            _targetKey2Blend = 0f;
        }
        else
        {
            // Faz 2: 15% -> 0%
            // bitti: 100 -> 40
            // Key 2: 0 -> 80
            float phase2Progress = Mathf.InverseLerp(15f, 0f, fillPercentage); // 0 to 1
            _targetBittiBlend = Mathf.Lerp(100f, 40f, phase2Progress);
            _targetKey2Blend = Mathf.Lerp(0f, 80f, phase2Progress);
        }
    }
    
    // BlendShape'leri yumuşatılmış şekilde güncelle
    void UpdateAmmoBlendShapes()
    {
        if (ammoBlendShapeRenderer == null) return;
        
        // Yumuşatılmış geçiş
        _currentBittiBlend = Mathf.Lerp(_currentBittiBlend, _targetBittiBlend, Time.deltaTime * blendShapeSmoothSpeed);
        _currentKey2Blend = Mathf.Lerp(_currentKey2Blend, _targetKey2Blend, Time.deltaTime * blendShapeSmoothSpeed);
        
        // BlendShape'leri uygula
        ammoBlendShapeRenderer.SetBlendShapeWeight(0, _currentBittiBlend); // "bitti" index 0 varsayımı
        ammoBlendShapeRenderer.SetBlendShapeWeight(1, _currentKey2Blend);  // "Key 2" index 1 varsayımı
    }
    
    // Lazer rengini overheat'e göre güncelle
    void UpdateLaserColor()
    {
        if (laserLine == null) return;
        
        // Overheat oranına göre renk
        float heatRatio = Mathf.Clamp01(continuousFireTime / (overheatThreshold * heatColorScale));
        
        Color laserColor;
        if (heatRatio < 0.33f)
        {
            float t = heatRatio / 0.33f;
            laserColor = Color.Lerp(new Color(0.3f, 0.7f, 1f), new Color(1f, 1f, 0.3f), t); // Mavi -> Sarı
        }
        else if (heatRatio < 0.66f)
        {
            float t = (heatRatio - 0.33f) / 0.33f;
            laserColor = Color.Lerp(new Color(1f, 1f, 0.3f), new Color(1f, 0.3f, 0f), t); // Sarı -> Turuncu
        }
        else
        {
            float t = (heatRatio - 0.66f) / 0.34f;
            laserColor = Color.Lerp(new Color(1f, 0.3f, 0f), new Color(1f, 0f, 0f), t); // Turuncu -> Kırmızı
        }
        
        laserColor.a = 1f;
        laserLine.startColor = laserColor;
        laserLine.endColor = laserColor;
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
