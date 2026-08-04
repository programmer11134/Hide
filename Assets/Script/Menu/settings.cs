using UnityEngine;

public class settings : MonoBehaviour
{
    
    public GameObject set;
    public GameObject menu;

    public GameObject Graphics;
    public GameObject Contols;
    public GameObject Sound;


    private void Start()
    {
        set.SetActive(false);
        menu.SetActive(true);
        Graphics.SetActive(false);
        Contols.SetActive(false);
        Sound.SetActive(false);
    }

    public void isOpenSettings()
    {
        set.SetActive(true);
        menu.SetActive(false);
        Graphics.SetActive(true);
        Contols.SetActive(false);
        Sound.SetActive(false);
    }
    public void isCloseSettings()
    {
        set.SetActive(false);
        menu.SetActive(true);
    }
    public void inGraphics()
    {
        Graphics.SetActive(true);
        Contols.SetActive(false);
        Sound.SetActive(false);
    }
    public void inControls()
    {
        Graphics.SetActive(false);
        Contols.SetActive(true);
        Sound.SetActive(false);
    }
    public void inSound()
    {
        Graphics.SetActive(false);
        Contols.SetActive(false);
        Sound.SetActive(true);
    }

}
