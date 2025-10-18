using UnityEngine;
using UnityEngine.UI;

// Ekranın kenarlarından ortaya doğru damar sprite'ı gösteren overlay.
// - Health = max -> invisible
// - Health = 0   -> fully visible
// - Hasar alındığında kısa bir pulse ekler, daha sonra yumuşakça hedef health-based alpha'ya döner.
public class DamageOverlay : MonoBehaviour
{
    public Image overlayImage; // fullscreen Image (vein sprite)
    public float fadeSpeed = 3f; // hedefe yumuşak yaklaşma hızı
    public float pulseMultiplier = 1.0f; // hasarın alpha'ya çevirme katsayısı (hasar / maxHealth * pulseMultiplier)

    private PlayerHealth playerHealth;
    private float currentAlpha = 0f;
    private float pulseAlpha = 0f; // hasar pulse'unun katkısı

    void Start()
    {
        if (overlayImage == null)
        {
            Debug.LogWarning("DamageOverlay: overlayImage atanmamış.");
            enabled = false;
            return;
        }

        // Başlangıçta görünmez
        var c = overlayImage.color;
        c.a = 0f;
        overlayImage.color = c;

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

        // Eğer özel bir materyal ile Shader Graph kullanacaksanız, Image'in materyalini instancela
        if (overlayImage.material != null)
        {
            overlayImage.material = new Material(overlayImage.material);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnTakeDamage -= OnDamage;
    }

    void OnDamage(float amount)
    {
        if (playerHealth == null || playerHealth.maxHealth <= 0f) return;

        // Pulse miktarı: hasarın maxHealth'e oranı * multiplier
        float add = (amount / playerHealth.maxHealth) * pulseMultiplier;
        pulseAlpha = Mathf.Clamp01(pulseAlpha + add);
    }

    void Update()
    {
        if (overlayImage == null) return;

        // Baz alpha: health'e göre (100 -> 0, 0 -> 1)
        float baseAlpha = 1f;
        if (playerHealth != null && playerHealth.maxHealth > 0f)
        {
            baseAlpha = 1f - (playerHealth.CurrentHealth / playerHealth.maxHealth);
        }

        // Pulse alpha yavaşça azalır
        pulseAlpha = Mathf.Lerp(pulseAlpha, 0f, Time.deltaTime * (fadeSpeed * 1.5f));

        // Hedef alpha = baseAlpha + pulseAlpha
        float targetAlpha = Mathf.Clamp01(baseAlpha + pulseAlpha);

        // Yumuşak geçiş
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Shader Graph veya normal Image renk ile uygulama
        var mat = overlayImage.material;
        if (mat != null && mat.HasProperty("_Alpha"))
        {
            mat.SetFloat("_Alpha", currentAlpha);
        }
        else
        {
            Color c = overlayImage.color;
            c.a = currentAlpha;
            overlayImage.color = c;
        }
    }
}

