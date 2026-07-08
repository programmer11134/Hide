using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menupanel : MonoBehaviour
{
    public void Exite()
    {
        Application.Quit();
    }
   public void Game()
    {


        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
}

