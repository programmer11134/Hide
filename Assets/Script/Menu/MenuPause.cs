using UnityEngine;

public class MenuPause : MonoBehaviour
{
    public GameObject PanelPauseMenu;
    bool isESC;

    private void Start()
    {
        isESC = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isESC)
        {
            PanelPauseMenu.SetActive(true);
            isESC = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape) && isESC)
        {
            PanelPauseMenu.SetActive(false);
            isESC = false;
        }

    }

}
