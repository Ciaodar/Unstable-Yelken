using UnityEngine;
using System; // Ekledik

// Bu component'in olduğu objeye bir CharacterController eklenmesini zorunlu kıl.
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 12f; // Karakterin yürüme hızı
    public float gravity = -9.81f; // Yer çekimi kuvveti

    [Header("Ölüm Ayarları")]
    [Tooltip("Karakterin altındaki ölüm seviyesi (Y koordinatı)")]
    public float deathYLevel = -50f; // Örneğin Y = -50'nin altına düşerse ölsün

    private CharacterController controller;
    private PlayerHealth playerHealth; // PlayerHealth script'ine referans
    private Vector3 velocity; // Yer çekiminden kaynaklanan dikey hız
    private FootstepManager footstepManager;

    [Header("Kafa Sallanması (Head Bob) Ayarları")]
    public bool enableHeadBob = true;
    public float bobFrequency = 1.5f; // Sallanma hızı (saniyede kaç döngü)
    public float bobAmplitudeX = 0.05f; // Yatay (X) sallanma miktarı
    public float bobAmplitudeY = 0.1f; // Dikey (Y) sallanma miktarı
    public Transform cameraContainer; // Sallanma uygulanacak kamera objesi (Örn: FPS Camera)

   
    private float bobTimer; // Sallanma zamanlayıcısı

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>(); 
        footstepManager = GetComponent<FootstepManager>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerMovement script'i, PlayerHealth script'ini bulamadı! Lütfen ekleyin.");
        }
        
        // Kamera Container kontrolü
        if (cameraContainer == null)
        {
            Debug.LogError("Head Bobbing için kamera container atanmamış!");
        }
    }

    void Update()
    {
        // === 1. DÜŞÜŞ KONTROLÜ (YENİ MEKANİK) ===
        // Karakterin Y pozisyonu ölüm seviyesinin altına düştüyse ve hala ölmediyse
        if (transform.position.y < deathYLevel && playerHealth.CurrentHealth > 0)
        {
            // Karakteri anında öldür
            playerHealth.TakeDamage(playerHealth.maxHealth); 
            // Veya sadece playerHealth.Die(); fonksiyonunu çağırabilirsiniz. 
            // (TakeDamage ile çağırmak, hasar olaylarının tetiklenmesini sağlar.)
            return; // Ölüm sonrası hareket hesaplamalarına devam etme
        }


        // === 2. HAREKET KODU (MEVCUT KODUNUZ) ===
        
        // Klavyeden W, A, S, D girdilerini al (yatay ve dikey eksen)
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   

        // Hareket vektörünü oluştur. 
        Vector3 move = transform.right * x + transform.forward * z;

        // Hareket vektörünü hızla ve zamanla çarparak uygula
        controller.Move(move * speed * Time.deltaTime);

        // Yer çekimini uygula
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Eğer karakter yerdeyse yer çekimi hızını sıfırla ki sürekli artmasın.
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }
        
        if (enableHeadBob && cameraContainer != null)
        {
            HandleHeadBobbing();
        }
    }
    
    
    private void HandleHeadBobbing()
    {
        // Yürüme girdilerini alıyoruz (klavye hareket ediyor mu?)
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        // Karakterimiz yerdeyse ve hareket ediyorsa (W, A, S, D basılıysa)
        if (controller.isGrounded && (xInput != 0 || zInput != 0))
        {
            if (footstepManager != null)
            {
                footstepManager.TryPlayFootstep();
            }
            // Yürüme seslerini de burada tetikleyeceğiz (2. Bölümde)
            
            // Timer'ı artır
            bobTimer += Time.deltaTime * bobFrequency;

            // Sinüs dalgaları ile X ve Y pozisyonlarını hesapla
            float bobX = Mathf.Sin(bobTimer) * bobAmplitudeX;
            float bobY = (Mathf.Cos(bobTimer * 2f) * 0.5f + 0.5f) * bobAmplitudeY; 
            // Cos(2f) + 0.5f: Yürürken zıplama hissi için daha hızlı yukarı/aşağı hareketi sağlar

            // Kamerayı sallanan pozisyona taşı
            cameraContainer.localPosition = new Vector3(bobX, bobY, 0f);
            
            
        }
        else
        {
            // Karakter duruyorsa veya havadaysa, sallanma hareketini yumuşakça sıfırla
            if (bobTimer > 0)
            {
                cameraContainer.localPosition = Vector3.Lerp(cameraContainer.localPosition, Vector3.zero, Time.deltaTime * bobFrequency);
                bobTimer = 0;
            }
        }
    }
}