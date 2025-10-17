using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3f; // Düşmanın hareket hızı
    private Transform player; // Oyuncunun pozisyonunu tutmak için

    void Start()
    {
        // Oyunu bul ve "Player" tag'ine sahip olan objenin Transform'unu al.
        // BU ADIM ÖNEMLİ! Az sonra oyuncumuza bu tag'i vereceğiz.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        // Eğer oyuncu hala hayattaysa ona doğru hareket et
        if (player != null)
        {
            // Yönü hesapla: Hedef - Kendi Pozisyonum
            Vector3 direction = (player.position - transform.position).normalized;

            // Hızla ve zamanla çarparak pozisyonu güncelle
            transform.position += direction * speed * Time.deltaTime;

            // Düşmanın sürekli oyuncuya bakmasını sağla (isteğe bağlı)
            transform.LookAt(player);
        }
    }
}