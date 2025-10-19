using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance; //Singleton örneği
    public int EnemiesAlive = 0; // Canlı düşman sayısını takip etmek için değişken
    [Header("Dalga Ayarları")]
    public Transform enemyPrefab; // Hangi düşmanı spawn edeceğiz?
    public Transform[] spawnPoints; // Nerelerde spawn edeceğiz?
    public int maxEnemyNumber = 15; // Maksimum canlı düşman sayısı

    public float timeBetweenWaves = 5f; // Dalgalar arası bekleme süresi
    private float waveCountdown; // Bir sonraki dalga için geri sayım

    private int waveNumber = 1; // Mevcut dalga numarası
    private int enemiesToSpawn; // O dalgada spawn edilecek düşman sayısı

    void Start()
    {
        if (instance == null)
        {
            instance = this; // Singleton ataması
        }
        else
        {
            Destroy(gameObject); // İkinci bir örnek varsa yok et
        }
        
        // İlk dalga için düşman sayısını belirle
        enemiesToSpawn = 5; 
        waveCountdown = 2f; // Oyun başlar başlamaz ilk dalga 2 saniye sonra gelsin.
    }

    void Update()
    {
        // Eğer hayatta hiç düşman kalmadıysa, yeni dalga için geri sayımı başlat.
        if (!EnemyIsAlive())
        {
            if (waveCountdown <= 0f)
            {
                // Geri sayım bitti, yeni dalgayı başlat!
                StartCoroutine(SpawnWave());
                waveCountdown = timeBetweenWaves; // Sayacı sıfırla
            }
            else
            {
                // Geri sayımı azalt
                waveCountdown -= Time.deltaTime;
            }
        }
    }

    // Coroutine: Belirli aralıklarla işlem yapmamızı sağlayan özel bir fonksiyon.
    IEnumerator SpawnWave()
    {
        Debug.Log("Dalga Başlıyor: " + waveNumber);

        for (int i = 0; i < enemiesToSpawn;)
        {
            if (EnemiesAlive< maxEnemyNumber)
            {
                i++;
                SpawnEnemy();
                // Düşmanları ardı ardına değil, 1 saniye arayla spawn et
                
            }
            yield return new WaitForSeconds(1f); 
        }

        // Dalga bitti, bir sonraki dalgayı zorlaştır.
        waveNumber++;
        enemiesToSpawn = (int)(enemiesToSpawn*Random.Range(0.8f, 1.9f));
    }

    void SpawnEnemy()
    {
        // Rastgele bir spawn noktası seç
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Düşmanı o noktada yarat
        Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
        Debug.Log("Bir düşman spawn oldu!");
        EnemiesAlive++; // Yeni bir düşman spawn olduğunda sayacı artır
    }

    // Sahnede "Enemy" tag'ine sahip bir obje var mı diye kontrol et.
    bool EnemyIsAlive()
    {
        return EnemiesAlive > 0; // Eğer EnemiesAlive sıfırdan büyükse, düşman var demektir.
    }
}