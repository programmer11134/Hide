using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    public static Fade instance;

    public CanvasGroup fadeGroup;
    public float fadeSpeed = 1f;

    private bool isFading = false;

    void Awake()
    {
        instance = this;
    }

    public void StartFadeAndRestart()
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndRestart());
        }
    }

    System.Collections.IEnumerator FadeAndRestart()
    {
        isFading = true;
        float alpha = 0f;

        // плавно увеличиваем прозрачность (альфу) до 1 (полностью чёрный)
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeGroup.alpha = alpha;
            yield return null; // ждём следующий кадр
        }

        // экран полностью чёрный — перезапускаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}