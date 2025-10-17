using UnityEngine;

// Bu component'in olduğu objeye bir CharacterController eklenmesini zorunlu kıl.
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 12f; // Karakterin yürüme hızı
    public float gravity = -9.81f; // Yer çekimi kuvveti

    private CharacterController controller;
    private Vector3 velocity; // Yer çekiminden kaynaklanan dikey hız

    void Start()
    {
        // Script başladığında CharacterController bileşenini bul ve ata.
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Klavyeden W, A, S, D girdilerini al (yatay ve dikey eksen)
        float x = Input.GetAxis("Horizontal"); // A/D tuşları için -1 ile 1 arası değer
        float z = Input.GetAxis("Vertical");   // W/S tuşları için -1 ile 1 arası değer

        // Hareket vektörünü oluştur. 
        // transform.right: Karakterin sağına doğru yönü
        // transform.forward: Karakterin baktığı yöne doğru yönü
        // Bu sayede karakterimiz her zaman baktığı yöne doğru hareket eder.
        Vector3 move = transform.right * x + transform.forward * z;

        // Hareket vektörünü hızla ve zamanla çarparak uygula
        controller.Move(move * speed * Time.deltaTime);

        // Yer çekimini uygula
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Eğer karakter yerdeyse yer çekimi hızını sıfırla ki sürekli artmasın.
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Küçük bir negatif değerde tutmak daha stabil çalışmasını sağlar.
        }
    }
}