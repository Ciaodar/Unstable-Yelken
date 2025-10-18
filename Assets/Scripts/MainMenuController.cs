using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject creditsMenu;

    void Start()
    {
        ShowMainMenu();
    }
    
    public void StartGame()
    {
        Debug.Log("Oyun başlatılıyor...");
        // Burada sahne yükleme kodu
        SceneManager.LoadScene(1); // 1 numaralı sahneyi yükle (oyun sahnesi)
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
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
