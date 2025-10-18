using UnityEngine;
using UnityEngine.AI; // NavMeshAgent sınıfını kullanmak için gerekli

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent; 
    private Transform player; 
    private Animator animator; // Animasyonları kontrol etmek için

    [Header("Ayarlar")]
    public string playerTag = "Player"; // Oyuncunun tag'i

    [Header("Saldırı Ayarları")]
    [Tooltip("Düşmanın saldırmaya başlayacağı maksimum mesafe.")]
    public float attackRange = 2f; 
    [Tooltip("Saniyede kaç kez saldıracağı (Örn: 1.0f 1 saniyede bir saldırı).")]
    public float attackRate = 1f; 
    [Tooltip("Her saldırıda oyuncuya verilecek hasar miktarı.")]
    public float attackDamage = 10f; 
    [Tooltip("Saldırı animasyonu başladıktan ne kadar sonra hasar verileceği (animasyonun vuruş anına göre ayarlanır).")]
    public float damageDelay = 0.5f;

    private float _nextAttackTime = 0f; // Bir sonraki saldırının yapılabileceği zaman
    private float _distanceToPlayer; // Oyuncuya olan anlık mesafe
    private bool _isAttacking = false;

    void Start()
    {
        // 1. Gerekli bileşenleri al
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Animator bileşenini almayı unutmayın!

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

        // Bileşen kontrolleri
        if (agent == null)
        {
            Debug.LogError(gameObject.name + " objesi üzerinde NavMeshAgent bileşeni bulunamadı!");
        }
        if (animator == null)
        {
            Debug.LogError(gameObject.name + " objesi üzerinde Animator bileşeni bulunamadı! Saldırı/Yürüme animasyonları çalışmayacak.");
        }
    }

    void Update()
    {
        if (agent == null || player == null) return;

        // Oyuncuya olan mesafeyi hesapla
        _distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Eğer saldırı menzilindeysek VEYA şu anda saldırı animasyonu çalışıyorsa
        if (_distanceToPlayer <= attackRange || _isAttacking)
        {
            // !!! TEMEL DURDURMA MANTIĞI !!!
            // Hareketi durdur, NavMeshAgent'ın hedef takibini devre dışı bırak.
            agent.isStopped = true; 
            
            // Yürüme animasyonunu durdur/Idle animasyonuna geç
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
            
            // Eğer sadece menzilde duruyorsak ve saldırı sırası gelmişse saldır
            if (!_isAttacking)
            {
                // Saldırı zamanı kontrolü
                if (Time.time >= _nextAttackTime)
                {
                    // Oyuncuya doğru dön
                    RotateTowardsPlayer();
                    AttackPlayer();
                }
            }
            
            // Not: Saldırı sırasında dönme hareketini kaldırmak isterseniz
            // RotateTowardsPlayer();
            // satırını AttackPlayer() metodu içine taşıyabilirsiniz.
        }
        else
        {
            // TAKİP (SALDIRI MENZİLİ DIŞINDA)
            
            // Hareketi serbest bırak
            agent.isStopped = false;
            _isAttacking = false; // Saldırı bitti, takip etme moduna geç
            
            // Oyuncuyu hedefle
            agent.SetDestination(player.position);
            
            // Yürüme animasyonunu tetikle
            if (animator != null)
            {
                bool isMoving = agent.velocity.sqrMagnitude > 0.01f;
                animator.SetBool("IsMoving", isMoving);
            }
        }
    }

    // Saldırı metodu
    void AttackPlayer()
    {
        _isAttacking = true; // Saldırıya başladık! Bu, Update'te hareketi engeller.

        // Bir sonraki saldırı zamanını ayarla
        _nextAttackTime = Time.time + 1f / attackRate;

        // Saldırı animasyonunu tetikle
        if (animator != null)
        {
            animator.SetTrigger("Attack"); 
        }

        // Hasar verme ve animasyon bitişini yönetme
        // Hasar verme işlemini geciktir
        Invoke(nameof(DealDamage), damageDelay);
        
        // Animasyonun bitiş süresine yakın bir zamanda _isAttacking bayrağını sıfırla
        // NOT: Bu süreyi saldırı animasyonunuzun süresi kadar ayarlayın!
        float animationDuration = 1f / attackRate; // Basit bir tahmin
        Invoke(nameof(EndAttack), animationDuration); 
    }

    void EndAttack()
    {
        _isAttacking = false;
        // Eğer hala menzil dışındaysak, Update bir sonraki frame'de takibe başlayacaktır.
    }
    
    // Oyuncuya hasar verme metodu (Invoke ile çağrılır)
    void DealDamage()
    {
        // Hasar vermeden önce oyuncunun hala menzil içinde olup olmadığını kontrol et
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f) // Biraz tolerans ekle
        {
            // Oyuncunun can scriptini almaya çalış
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("Oyuncu objesi üzerinde PlayerHealth script'i bulunamadı!");
            }
        }
    }
    
    // Oyuncuya doğru yavaşça dönme
    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        // Sadece XZ düzleminde dönüş yapmak için Y eksenini sıfırla
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        // Yumuşak dönüş
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}