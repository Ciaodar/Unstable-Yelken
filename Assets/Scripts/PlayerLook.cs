using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    // TEMEL DEĞİŞKENLER
    [Header("Hassasiyet Ayarları")]
    public float baseMouseSensitivity = 100f; // Temel fare hassasiyetimiz.
    
    [Header("Referanslar")]
    public Transform playerBody; // Oyuncunun gövdesi (Capsule)
    public Weapon currentWeapon; // Mevcut silahımızın referansı

    private float xRotation = 0f; // Yukarı/aşağı bakış açısını saklamak için.

    void Start()
    {
        // Oyuna başlarken fare imlecini ekranın ortasına kilitle ve gizle.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Fare girdilerini al
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime;

        // Silahın durumuna göre anlık hassasiyeti hesapla
        // BU KISIM MEKANİĞİN KALBİ! Silah script'inden gelen çarpanı kullanıyoruz.
        float currentSensitivity = baseMouseSensitivity * currentWeapon.GetSensitivityMultiplier();
        
        // Yatayda (sağa-sola) tüm oyuncu gövdesini döndür
        playerBody.Rotate(Vector3.up * mouseX * currentSensitivity);

        // Dikeyde (aşağı-yukarı) sadece kamerayı döndür
        xRotation -= mouseY * currentSensitivity;
        
        // Kameranın 180 derece dönüp tepe taklak olmasını engelle (clamping)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Hesaplanan dikey açıyı kameraya uygula
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}