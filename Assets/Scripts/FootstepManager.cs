using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips; // Rastgele çalınacak yürüme sesleri dizisi

    [Header("Ses Zamanlaması")]
    public float stepInterval = 0.5f; // Saniye cinsinden adım aralığı (Ne kadar hızlı ses çalınsın)
    private float stepTimer; // Zamanlayıcı

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (audioSource == null)
        {
            Debug.LogError("AudioSource bileşeni atanmamış/bulunamadı!");
        }
    }

    // PlayerMovement script'i tarafından çağrılacak fonksiyon
    public void TryPlayFootstep()
    {
        // Eğer ses kaynağımız varsa ve zaman geldiğinde
        if (audioSource != null && Time.time > stepTimer)
        {
            PlayRandomFootstep();
            // Yeni adımı tetiklemek için bir sonraki zamanı ayarla
            stepTimer = Time.time + stepInterval;
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips.Length == 0) return;

        // Diziden rastgele bir ses seç ve çal
        int randomIndex = Random.Range(0, footstepClips.Length);
        audioSource.PlayOneShot(footstepClips[randomIndex]);
    }
}