using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Dalga Ayarları")]
    public Transform enemyPrefab; // Hangi düşmanı spawn edeceğiz?
    public Transform[] spawnPoints; // Nerelerde spawn edeceğiz?

    public float timeBetweenWaves = 5f; // Dalgalar arası bekleme süresi
    private float waveCountdown; // Bir sonraki dalga için geri sayım

    private int waveNumber = 1; // Mevcut dalga numarası
    private int enemiesToSpawn; // O dalgada spawn edilecek düşman sayısı

    void Start()
    {
        // İlk dalga için düşman sayısını belirle
        enemiesToSpawn = 3; 
        waveCountdown = 3f; // Oyun başlar başlamaz ilk dalga 3 saniye sonra gelsin.
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

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            // Düşmanları ardı ardına değil, 1 saniye arayla spawn et
            yield return new WaitForSeconds(1f); 
        }

        // Dalga bitti, bir sonraki dalgayı zorlaştır.
        waveNumber++;
        enemiesToSpawn += 2; // Her yeni dalgada 2 düşman daha fazla gelsin.
    }

    void SpawnEnemy()
    {
        // Rastgele bir spawn noktası seç
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Düşmanı o noktada yarat
        Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
        Debug.Log("Bir düşman spawn oldu!");
    }

    // Sahnede "Enemy" tag'ine sahip bir obje var mı diye kontrol et.
    bool EnemyIsAlive()
    {
        if (GameObject.FindGameObjectWithTag("Enemy"))
        {
            return true;
        }
        return false;
    }
}