using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menupanel : MonoBehaviour
{
    public void Exite()
    {
        Debug.Log("Кнопка выхода нажата!"); // Это появится в консоли

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Остановит игру в редакторе
#else
        Application.Quit();
#endif
    }
    public void Game()
    {


        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
}

