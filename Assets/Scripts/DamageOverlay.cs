using UnityEngine;
using UnityEngine.UI;

// Ekranın kenarlarından ortaya doğru damar sprite'ı gösteren overlay.
// - Health = max -> invisible
// - Health = 0   -> fully visible
// - Hasar alındığında kısa bir pulse ekler, daha sonra yumuşakça hedef health-based alpha'ya döner.
public class DamageOverlay : MonoBehaviour
{
    public SpriteRenderer overlaySprite; // kameranın önündeki sprite

    [Header("Alpha / Scale Ayarları")]
    public float fadeSpeed = 3f; // hedefe yumuşak yaklaşma hızı
    public float scaleMax = 0.06f; // alpha=0 iken scale
    public float scaleMin = 0.03f; // alpha=1 iken scale
    public float scaleSmoothSpeed = 4f; // scale yumuşatma

    // maksimum _Alpha değeri (0 .. maxAlpha)
    public float maxAlpha = 1.5f;

    [Header("Pulse Ayarları")]
    [Tooltip("İki pulse arasında izin verilen minimum süre (saniye). Sürekli hasarda jitteri azaltmak için")]
    public float minPulseInterval = 0.15f;
    [Tooltip("Hasarın alpha'a dönüştürme katsayısı")]
    public float pulseMultiplier = 1.0f;
    [Tooltip("Hasarın scale'e eklediği miktar")]
    public float pulseScaleAmount = 0.02f;
    [Tooltip("Pulse'un sönme hızı")]
    public float pulseDecayRate = 3f;

    // önceki base alpha (0..1) takip için
    private float _prevBaseAlphaNorm = 0f;

    private PlayerHealth playerHealth;
    private float currentAlpha = 0f;
    private float currentScale = 0.06f;
    private float targetScale = 0.06f;
    private float pulseAlpha = 0f; // pulse katkısı (0..1)
    private float pulseScale = 0f;
    private float _bufferedDamage = 0f;
    private float _lastPulseTime = -999f;

    private Material spriteMaterialInstance;

    void Start()
    {
        if (overlaySprite == null)
        {
            Debug.LogWarning("DamageOverlay: overlaySprite atanmamış.");
            enabled = false;
            return;
        }

        if (overlaySprite != null)
        {
            var sc = overlaySprite.color;
            sc.a = 0f;
            overlaySprite.color = sc;
            if (overlaySprite.material != null)
            {
                spriteMaterialInstance = new Material(overlaySprite.material);
                overlaySprite.material = spriteMaterialInstance;
            }
            // başlangıç scale
            currentScale = scaleMax;
            if (overlaySprite.transform != null) overlaySprite.transform.localScale = Vector3.one * currentScale;
        }

        // PlayerHealth'i bul (Player tag'li obje)
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerHealth = p.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnTakeDamage += OnDamage;
        }
        else
        {
            Debug.LogWarning("DamageOverlay: PlayerHealth bulunamadı. Player objesine 'Player' tag'i verin veya script'e PlayerHealth referansı atayın.");
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnTakeDamage -= OnDamage;
    }

    void OnDamage(float amount)
    {
        if (playerHealth == null || playerHealth.maxHealth <= 0f) return;
        // Hasarı buffer'la, Update'te belirli aralıklarla işle
        _bufferedDamage += amount;
    }

    void Update()
    {
        // Baz alpha: health'e göre (100 -> 0, 0 -> 1)
        float baseAlpha = 1f;
        if (playerHealth != null && playerHealth.maxHealth > 0f)
        {
            baseAlpha = 1f - (playerHealth.CurrentHealth / playerHealth.maxHealth);
        }

        // normalize base alpha 0..1
        float baseAlphaNorm = Mathf.Clamp01(baseAlpha);
        // önceki base alpha'yı local olarak sakla (karşılaştırmalar için)
        float prevBaseAlpha = _prevBaseAlphaNorm;
        // scaled base alpha for _Alpha property (0..maxAlpha)
        float baseAlphaScaled = baseAlphaNorm * maxAlpha;

        // Apply base alpha: if baseAlpha increased (health dropped) apply immediately; if decreased, smooth it
        float appliedBaseAlphaScaled;
        if (baseAlphaNorm >= prevBaseAlpha)
        {
            // health decreased -> alpha increased: immediate
            appliedBaseAlphaScaled = baseAlphaScaled;
        }
        else
        {
            // health increased -> alpha decreased: smooth the decrease
            float prevScaled = prevBaseAlpha * maxAlpha;
            appliedBaseAlphaScaled = Mathf.Lerp(prevScaled, baseAlphaScaled, Time.deltaTime * fadeSpeed);
        }

        // Eğer bufferedDamage doluysa ve minPulseInterval geçtiyse, pulse'u tetikle
        if (_bufferedDamage > 0f && Time.time - _lastPulseTime >= minPulseInterval && playerHealth != null && playerHealth.maxHealth > 0f)
        {
            float dmg = _bufferedDamage;
            _bufferedDamage = 0f;
            _lastPulseTime = Time.time;

            float add = (dmg / playerHealth.maxHealth) * pulseMultiplier;
            pulseAlpha = Mathf.Clamp01(pulseAlpha + add);
            pulseScale += (dmg / playerHealth.maxHealth) * pulseScaleAmount;
            // pulseScale clamp so it doesn't push below scaleMin
            pulseScale = Mathf.Clamp(pulseScale, 0f, scaleMax - scaleMin);
        }

        // Pulse alpha yavaşça azalır (kontrollü)
        pulseAlpha = Mathf.Lerp(pulseAlpha, 0f, Time.deltaTime * pulseDecayRate);

        // Hedef alpha = applied base scaled + pulse katkısı (pulse 0..1 mapped to maxAlpha)
        float targetAlpha = appliedBaseAlphaScaled + (pulseAlpha * maxAlpha);
        targetAlpha = Mathf.Clamp(targetAlpha, 0f, maxAlpha);

        // Apply immediately (no extra smoothing on combined value)
        currentAlpha = targetAlpha;
        if (overlaySprite != null)
        {
            if (spriteMaterialInstance != null && spriteMaterialInstance.HasProperty("_Alpha")) spriteMaterialInstance.SetFloat("_Alpha", currentAlpha);
            else
            {
                Color sc = overlaySprite.color;
                sc.a = currentAlpha;
                overlaySprite.color = sc;
            }
        }

        // Scale hedefi health'e göre: alpha=0 -> scaleMax, alpha=1 -> scaleMin
        targetScale = Mathf.Lerp(scaleMax, scaleMin, baseAlphaNorm);
        // Apply pulseScale (pulse reduces scale)
        float pulseAdjustedScale = targetScale - pulseScale;
        // Ensure scale does not go below scaleMin
        pulseAdjustedScale = Mathf.Clamp(pulseAdjustedScale, scaleMin, scaleMax);
        // If baseAlpha increased (health dropped) we want immediate scale decrease, else smooth increase
        if (baseAlphaNorm >= prevBaseAlpha)
        {
            // immediate (scale may decrease)
            currentScale = pulseAdjustedScale;
        }
        else
        {
            // smooth
            currentScale = Mathf.Lerp(currentScale, pulseAdjustedScale, Time.deltaTime * scaleSmoothSpeed);
        }
        // pulseScale yavaşça azalır
        pulseScale = Mathf.Lerp(pulseScale, 0f, Time.deltaTime * pulseDecayRate);

        if (overlaySprite != null && overlaySprite.transform != null)
        {
            overlaySprite.transform.localScale = Vector3.one * currentScale;
        }

        // şimdi önceki base alpha değerini güncelle
        _prevBaseAlphaNorm = baseAlphaNorm;
    }
}
