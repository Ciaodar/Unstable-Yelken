using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance; //Singleton örneği
    public int EnemiesAlive = 0; // Canlı düşman sayısını takip etmek için değişken
    
    // GameManager'ın erişmesi için dalga numarasını public yapıyoruz
    [HideInInspector] // Inspector'da görünmesini istemiyorsanız (GameManager zaten kullanıyor)
    public int currentWave = 1; // Mevcut dalga numarası
    
    [Header("Dalga Ayarları")]
    public Transform[] enemyPrefabs; // Hangi düşmanı spawn edeceğiz?
    public Transform[] spawnPoints; // Nerelerde spawn edeceğiz?
    public int maxEnemyNumber = 15; // Maksimum canlı düşman sayısı

    public float timeBetweenWaves = 5f; // Dalgalar arası bekleme süresi
    private float waveCountdown; // Bir sonraki dalga için geri sayım

    private int enemiesToSpawn; // O dalgada spawn edilecek düşman sayısı

    void Awake()
    {
        if (instance == null)
        {
            instance = this; // Singleton ataması
        }
        else
        {
            Destroy(gameObject); // İkinci bir örnek varsa yok et
        }
    }

    void Start()
    {
        currentWave = 1; // Başlangıç dalgası
        enemiesToSpawn = 5; 
        waveCountdown = 2f; // Oyun başlar başlamaz ilk dalga 2 saniye sonra gelsin.
    }

    void Update()
    {
        // Düşman sayısı kontrolü:
        // Eğer EnemiesAlive sıfırsa VE şu anda düşman spawn edilmiyorsa (waveCountdown > 0f)
        if (!EnemyIsAlive())
        {
            if (waveCountdown <= 0f)
            {
                StartCoroutine(SpawnWave());
                waveCountdown = timeBetweenWaves; 
            }
            else
            {
                waveCountdown -= Time.deltaTime;
            }
        }
    }

    IEnumerator SpawnWave()
    {
        Debug.Log("Dalga Başlıyor: " + currentWave);

        int spawnedThisWave = 0;

        // Belirlenen sayıda düşman spawn et
        while (spawnedThisWave < enemiesToSpawn)
        {
            // Eğer maksimum canlı düşman sınırına ulaştıysak beklemeye devam et.
            if (EnemiesAlive < maxEnemyNumber)
            {
                SpawnEnemy();
                spawnedThisWave++; // Spawn edilen sayıyı artır
            }
            // Spawn aralığı
            yield return new WaitForSeconds(1f); 
        }

        // Dalga bitti, bir sonraki dalgayı zorlaştır ve dalga numarasını artır.
        currentWave++; // <--- Burası PUAN ÇARPANI için kritik!
        
        // Yeni dalga için spawn edilecek düşman sayısını belirle
        enemiesToSpawn = Mathf.RoundToInt(enemiesToSpawn * Random.Range(1.2f, 1.6f)); // Daha agresif bir artış önerisi
    }

    void SpawnEnemy()
    {
        // Kontrol: Düşman prefab listesi boş mu?
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogError("Düşman Prefab listesi (enemyPrefabs) boş! Lütfen en az bir düşman ekleyin.");
            return;
        }
        
        // 1. Rastgele bir düşman prefabı seç
        // Random.Range(int min, int max) metodunda max değeri hariç tutulur, bu yüzden uzunluk doğru aralığı verir.
        Transform selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // 2. Rastgele bir spawn noktası seç
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 3. Seçilen düşmanı yarat
        Instantiate(selectedEnemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
        
        EnemiesAlive++; // Yeni bir düşman spawn olduğunda sayacı artır
        Debug.Log($"Bir düşman spawn oldu! Toplam canlı: {EnemiesAlive}");
    }
    bool EnemyIsAlive()
    {
        return EnemiesAlive > 0;
    }
}