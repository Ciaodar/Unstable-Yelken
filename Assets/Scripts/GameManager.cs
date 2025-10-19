using UnityEngine;
using TMPro; // TextMeshPro kullanıyorsanız bu kütüphaneyi eklemelisiniz

public class GameManager : MonoBehaviour
{
    // === SINGLETON INSTANCE ===
    public static GameManager Instance;

    // === VARSAYILAN PUAN VE SAYACLAR ===
    [Header("Puan Ayarları")]
    public int baseEnemyPoints = 10; // Öldürülen her düşman için temel puan
    private int totalEnemiesKilled = 0;
    
    // === Inspector'dan atayacağımız değişkenler ===
    [Header("UI Elemanları")]
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI scoreText;

    [Header("Oyun Değişkenleri")]
    public float timeSurvived = 0f;
    public int currentScore = 0;

    // Tek saniye kontrolü için
    private float updateTimer = 0f;
    private const float updateInterval = 0.1f; 

    private WaveSpawner waveSpawner;

    void Awake()
    {
        // Singleton Kuralı
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Diğer bileşenleri bul
        waveSpawner = FindObjectOfType<WaveSpawner>();
        if (waveSpawner == null)
        {
            Debug.LogError("Sahne üzerinde WaveSpawner script'i bulunamadı! Puan çarpanı doğru çalışmayabilir.");
        }
        
        // UI'yı başlangıçta ayarla
        UpdateScoreUI();
        UpdateTimerUI(); // Başlangıçta 00:00 olarak göstermek için
    }

    void Update()
    {
        // 1. Zamanı Güncelleme
        timeSurvived += Time.deltaTime;
        
        // Timer UI'ı güncelleme
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateTimerUI();
            updateTimer = 0f;
        }
    }
    
    // === YENİ PUANLAMA METODU ===
    public void AddScoreForKill()
    {
        totalEnemiesKilled++;
        
        // 1. Mevcut Dalga Sayısını Al (Dalga çarpanı için)
        // WaveSpawner'da 'public int currentWave;' değişkeninin olduğunu varsayarız.
        int currentWave = 1; // Başlangıç değeri
        if (waveSpawner != null) 
        {
             // currentWave'in 0'dan büyük olduğundan emin ol (ilk dalga 1 olmalı)
            currentWave = Mathf.Max(1, waveSpawner.currentWave); 
        }

        // 2. Çarpanlı Puanı Hesapla (Temel puan * Dalga Sayısı)
        int pointsToAdd = baseEnemyPoints * currentWave;

        // 3. Puanı Ekle
        currentScore += pointsToAdd;

        // 4. UI'yı Güncelle
        UpdateScoreUI();
        
        Debug.Log($"Düşman öldürüldü! Eklenen Puan: {pointsToAdd}, Yeni Toplam Puan: {currentScore}");
    }

    // Zaman UI'sını güncelleyen metot
    void UpdateTimerUI()
    {
        if (timerText == null) return;
        
        // Zamanı dakika ve saniye formatına çevirme
        int minutes = Mathf.FloorToInt(timeSurvived / 60F);
        int seconds = Mathf.FloorToInt(timeSurvived % 60F);
        
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    // Puan UI'sını güncelleyen metot
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}