using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Video işlemlerini kullanmak için ekleyin

public class MainMenuController : MonoBehaviour
{
    public static bool isIntroWatched = false; // Intro izlenme durumu
    // === Menü Objesi Değişkenleri ===
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject creditsMenu;

    // === Video ile İlgili Yeni Değişkenler ===
    // Video'nun oynatılacağı RawImage veya Canvas nesnesini buraya atayın
    public GameObject videoCanvas; 
    // Video Player bileşeninin bulunduğu nesneyi buraya atayın
    public VideoPlayer videoPlayer; 
    
    // PlayerPrefs'te izlenme durumunu kaydetmek için kullanılacak anahtar
    private const string IntroWatchedKey = "IntroWatched"; 

    void Start()
    {
        // Video bittiğinde çalışacak metodu (OnVideoFinished) dinlemeye başla
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        // Script yok edildiğinde dinlemeyi bırak, hata almamak için önemlidir
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void Update()
    {
        // Escape tuşuna basıldığında ana menüye dön
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMainMenu();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Enter tuşuna basıldığında oyunu başlat
            StartGameScene();
        }
        if (Input.GetKeyDown(KeyCode.Q) && videoCanvas.gameObject.activeSelf)
        {
            StartGameScene();
        }
    }

    // === BUTON FONKSİYONLARI ===

    public void StartGame()
    {
        // Intro'nun daha önce izlenip izlenmediğini kontrol et
        if (isIntroWatched)
        {
            StartGameScene();
            return;
        }
        isIntroWatched = true;
        //videoyu oynat
        Debug.Log("Intro ilk kez izleniyor, video oynatılıyor.");
        
        // Ana menüyü kapat ve video ekranını aç
        mainMenu.SetActive(false); 
        
        if (videoCanvas != null && videoPlayer != null)
        {
            videoCanvas.SetActive(true);
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("Video Canvas veya Video Player atanmamış. Doğrudan oyun başlatılıyor.");
            StartGameScene();
        }
        
    }
    
    // Video bittiğinde (loopPointReached) otomatik olarak çalışacak metot
    private void OnVideoFinished(VideoPlayer vp)
    {
        // Video oynatma ekranını kapat
        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false);
        }
        
        // Oyun sahnesini yükle
        StartGameScene();
    }
    
    // Gerçek sahne yükleme işlemini yapan metot
    private void StartGameScene()
    {
        Debug.Log("Oyun sahnesi yükleniyor... (1 numaralı sahne)");
        SceneManager.LoadScene(1); // 1 numaralı sahneyi yükle (oyun sahnesi)
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
        // Menüye dönerken video ekranının kapalı olduğundan emin ol
        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false);
        }
    }

    public void ShowSettingsMenu()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        creditsMenu.SetActive(false);
    }

    public void ShowCreditsMenu()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Oyun kapatılıyor...");
        Application.Quit();
    }
}