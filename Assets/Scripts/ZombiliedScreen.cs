using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ZombiliedScreen : MonoBehaviour
{
    public static bool isOutroWatched = false;
    [Header("Kaybetme Ekranı Objeleri")]
    [SerializeField]private List<GameObject> destroyList;
    public Image firstImage;   // önce görünecek image
    public Image secondImage;  // sonra görünecek image
    public GameObject actionButton; // 2 saniye sonra aktif olacak kısım 

    [Header("Güncellenecek Yazılar")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    
    [Header("Outro Video Ayarları")]
    // Video'nun oynatılacağı RawImage veya Canvas nesnesini buraya atayın
    public GameObject videoCanvas; 
    public VideoPlayer videoPlayer;

    
    Coroutine _running;

    // Public metod: diğer scriptler (ör. PlayerHealth) öldüğünde bu fonksiyonu çağırsın


    private void Start()
    {
        // Başlangıçta hedef image'lerin alpha'sını 0 yap
        SetImageAlpha(firstImage, 0f);
        SetImageAlpha(secondImage, 0f);
        if (actionButton != null) actionButton.SetActive(false);// butonu gizle
    }
    
    public void ReturnToOutro()
    {
        //outro izlendi mi kontrol et
        if (isOutroWatched)
        {
            ReturnToMainMenu();
            return;
        }
        isOutroWatched = true;
        
        //videoyu oynat
        Debug.Log("Intro ilk kez izleniyor, video oynatılıyor.");
        
        // Ana menüyü kapat ve video ekranını aç
        firstImage.gameObject.SetActive(false);
        secondImage.gameObject.SetActive(false);
        actionButton.SetActive(false);
        
        
        if (videoCanvas != null && videoPlayer != null)
        {
            videoCanvas.SetActive(true);
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("Video Canvas veya Video Player atanmamış. Doğrudan oyun başlatılıyor.");
            
        }
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    

    public void Show()
    {
        gameObject.SetActive(true);
        //Mouse cursorunu göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Destroy listedeki objeleri yok et
        foreach (var obj in destroyList)
        {
            Destroy(obj);
        }
        
        // Skor ve süre iletisini yerleştir
        scoreText.text = GameManager.Instance.currentScore.ToString();
        timeText.text = TimeSpan.FromSeconds(GameManager.Instance.timeSurvived).ToString(@"mm\:ss");
        
        // Eğer zaten oynuyorsa yenisini başlatma
        if (_running != null) StopCoroutine(_running);
        // Başlangıçta hedef image'lerin alpha'sını 0 yap
        SetImageAlpha(firstImage, 0f);
        SetImageAlpha(secondImage, 0f);
        if (actionButton != null) actionButton.SetActive(false);

        _running = StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // Fade first image 0 -> 1 in 1 second
        yield return StartCoroutine(FadeImageAlpha(firstImage, 0f, 1f, 1f));
        // Immediately fade second image 0 -> 1 in 1 second
        yield return StartCoroutine(FadeImageAlpha(secondImage, 0f, 1f, 1f));
        // Bekle 2 saniye
        yield return new WaitForSeconds(2f);
        if (actionButton != null) actionButton.SetActive(true);
        _running = null;
    }

    IEnumerator FadeImageAlpha(Image img, float from, float to, float duration)
    {
        if (img == null)
        {
            yield break;
        }

        float t = 0f;
        // ensure starting alpha
        SetImageAlpha(img, from);

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            SetImageAlpha(img, alpha);
            yield return null;
        }
        SetImageAlpha(img, to);
    }

    void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = Mathf.Clamp01(a);
        img.color = c;
    }
}
