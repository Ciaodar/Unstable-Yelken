using UnityEngine;
using UnityEngine.SceneManagement;

// PauseController: ESC ile oyunu durdurur / devam ettirir.
// Seçilen yöntem: Time.timeScale = 0 ile global pause (basit ve yaygın).
public class PauseController : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject crosshair; // Canvas içindeki Crosshair objesi
    public GameObject pauseMenuRoot; // PauseMenu root (BackgroundFade + PausePanel)

    [Header("Ayarlar")]
    public string mainMenuSceneName = "MainMenu"; // Main menu sahne ismi
    public bool pauseAudio = true; // pause sırasında audioListener'ı durdur

    bool _isPaused = false;

    void Start()
    {
        // Başlangıçta pause menüsünü kapat
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        // Cursor oyun başlarken kilitli olsun
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ESC ile toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) Resume(); else Pause();
        }
    }

    // Pause uygula: Time.timeScale = 0, UI göster, cursor aç
    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;

        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Resume: Time.timeScale = 1, UI gizle, cursor kilitle
    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;

        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Main menu'e dön (butona bağlayın)
    public void GoToMainMenu()
    {
        // Önce oyun devam eder halde olmalı
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        // Sahne yükle
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("PauseController: mainMenuSceneName ayarlı değil.");
        }
    }

    // Butonlar için erişilebilir fonksiyonlar
    public void OnResumeButton() => Resume();
    public void OnMainMenuButton() => GoToMainMenu();
}

