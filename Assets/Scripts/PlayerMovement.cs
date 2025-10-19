using UnityEngine;
using System; 

// Bu component'in olduğu objeye bir CharacterController eklenmesini zorunlu kıl.
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 12f; // Karakterin yürüme hızı
    [Tooltip("Yer çekimi kuvveti. Daha gerçekçi düşüş için -18f veya -25f gibi değerler kullanın.")]
    public float gravity = -18f; // Yer çekimi kuvveti (Inspector'da bu değeri yükseltmeyi unutmayın!)
    public float reloadSpeedMultiplier = 1.0f; // Hareket hızı çarpanı

    [Header("Zıplama Ayarları")]
    public float jumpHeight = 3f; // Karakterin ne kadar yüksek zıplayacağı

    [Header("Ölüm Ayarları")]
    [Tooltip("Karakterin altındaki ölüm seviyesi (Y koordinatı)")]
    public float deathYLevel = -50f; 

    private CharacterController controller;
    private PlayerHealth playerHealth; 
    private Vector3 velocity; // Yer çekiminden kaynaklanan dikey hız
    private FootstepManager footstepManager;
    private Weapon weapon; // Mühimmat/Silah scriptine referans

    [Header("Kafa Sallanması (Head Bob) Ayarları")]
    public bool enableHeadBob = true;
    public float bobFrequency = 1.5f; 
    public float bobAmplitudeX = 0.05f; 
    public float bobAmplitudeY = 0.1f; 
    public Transform cameraContainer; // Sallanma uygulanacak kamera objesi 

    private float bobTimer; // Sallanma zamanlayıcısı

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>(); 
        footstepManager = GetComponent<FootstepManager>();
        
        // FindObjectOfType kullanırken null kontrolü önemlidir.
        weapon = FindObjectOfType<Weapon>(); 

        if (playerHealth == null)
        {
            Debug.LogError("PlayerMovement script'i, PlayerHealth script'ini bulamadı! Lütfen ekleyin.");
        }
        if (cameraContainer == null)
        {
            Debug.LogError("Head Bobbing için kamera container atanmamış!");
        }
    }

    void Update()
    {
        // === 1. DÜŞÜŞ VE ÖLÜM KONTROLÜ ===
        if (transform.position.y < deathYLevel && playerHealth.CurrentHealth > 0)
        {
            playerHealth.TakeDamage(playerHealth.maxHealth); 
            return; 
        }

        // === 2. YERDE KONTROLÜ VE ZIPLAMA ===
        
        // Eğer karakter yerdeyse, yer çekimi hızını sıfırla/sabit tut.
        // Bu, düşüşü keskinleştirir ve isGrounded durumunu korur.
        if (controller.isGrounded)
        {
            if (velocity.y < 0) 
            {
                velocity.y = -2f; // Karakteri yere sabitlemek için hafifçe aşağı it
            }

            // ZIPLAMA KONTROLÜ
            if (Input.GetButtonDown("Jump"))
            {
                // Zıplama formülü: v = sqrt(h * -2 * g)
                // Bu, hedeflenen yüksekliğe ulaşmak için gereken başlangıç hızını hesaplar.
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }


        // === 3. YATAY HAREKET KODU ===
        
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   

        Vector3 move = transform.right * x + transform.forward * z;

        // Yeniden yükleme hızını kontrol ederek hareketi uygula
        float reloadMultiplier = (weapon != null && weapon.IsReloading) ? reloadSpeedMultiplier : 1f;
        float currentSpeed = speed * Time.deltaTime * reloadMultiplier;
        
        controller.Move(move * currentSpeed);

        // === 4. YER ÇEKİMİNİ UYGULAMA (Daima) ===
        // Yer çekimi (velocity.y) her frame'de artar/azalır.
        velocity.y += gravity * Time.deltaTime;
        
        // Dikey hareketi uygula.
        controller.Move(velocity * Time.deltaTime);

        
        // === 5. HEAD BOBBING (Kafa Sallanması) ===
        if (enableHeadBob && cameraContainer != null)
        {
            HandleHeadBobbing(x, z);
        }
    }
    
    
    private void HandleHeadBobbing(float xInput, float zInput)
    {
        // Kontrol: Sadece yerdeysek VE hareket ediyorsak sallan
        if (controller.isGrounded && (xInput != 0 || zInput != 0))
        {
            if (footstepManager != null)
            {
                footstepManager.TryPlayFootstep();
            }
            
            // Yürüme hızı çarpanı ile bobbing frekansını ayarla (isteğe bağlı)
            float currentBobFrequency = bobFrequency * (weapon != null && weapon.IsReloading ? reloadSpeedMultiplier : 1f);
            
            bobTimer += Time.deltaTime * currentBobFrequency;

            float bobX = Mathf.Sin(bobTimer) * bobAmplitudeX;
            float bobY = (Mathf.Cos(bobTimer * 2f) * 0.5f + 0.5f) * bobAmplitudeY; 

            cameraContainer.localPosition = new Vector3(bobX, bobY, 0f);
        }
        else
        {
            // Karakter duruyorsa veya havadaysa (zıplıyorsa), sallanmayı sıfırla
            if (cameraContainer.localPosition != Vector3.zero) 
            {
                // Yumuşakça (Lerp) sıfıra dön
                cameraContainer.localPosition = Vector3.Lerp(cameraContainer.localPosition, Vector3.zero, Time.deltaTime * 5f);
                
                // Yerdeyken durduysak bob timer'ı sıfırla
                if (controller.isGrounded)
                {
                   bobTimer = 0; 
                }
            }
        }
    }
}