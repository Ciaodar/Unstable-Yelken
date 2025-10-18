using UnityEngine;
using UnityEngine.AI; // NavMeshAgent sınıfını kullanmak için gerekli

public class EnemyAI : MonoBehaviour
{
    // Hız, ivme, vb. ayarları artık bu bileşen üzerinden kontrol ediliyor.
    private NavMeshAgent agent; 
    
    // Oyuncunun pozisyonunu tutmak için
    private Transform player; 

    [Header("Ayarlar")]
    public string playerTag = "Player"; // Oyuncunun tag'i

    void Start()
    {
        // 1. NavMeshAgent bileşenini al (Düşman objenizde kurulu olmalı!)
        agent = GetComponent<NavMeshAgent>();

        // 2. Oyuncuyu bul ve Transform'unu al.
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Sahne üzerinde '" + playerTag + "' tag'ine sahip bir oyuncu bulunamadı. Lütfen oyuncuya bu tag'i verin!");
        }

        // Eğer agent bileşeni yoksa hata ver (NavMesh kullanmak zorundayız)
        if (agent == null)
        {
            Debug.LogError(gameObject.name + " objesi üzerinde NavMeshAgent bileşeni bulunamadı!");
        }
    }

    void Update()
    {
        // 3. Eğer hem agent hem de oyuncu varsa
        if (agent != null && player != null)
        {
            // NavMesh Agent'a oyuncunun pozisyonunu hedef olarak ver.
            // Agent, bu hedefe NavMesh (mavi alan) üzerinden engellerden kaçınarak 
            // en kısa yolu otomatik olarak bulacak ve hareket edecektir.
            agent.SetDestination(player.position);
            
            // Not: LookAt komutuna gerek kalmaz, Agent otomatik olarak hedefe döner.
            // Ancak, sadece görsel amaçlı daha iyi bir dönüş isterseniz bu kısma ekleyebilirsiniz.
        }
    }
}