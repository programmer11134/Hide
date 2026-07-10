using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField] private Renderer playerRendere;

    private void Start()
    {
        Color rc = RandomColor();
        playerRendere.material.color = rc;
        Debug.Log(rc);
    }
     
    private Color RandomColor()
    {
        int random = Random.Range(0, 4);

        switch (random)
        {
            case 0:
                return Color.blue;

            case 2:
                return Color.red;

            case 3:
                return Color.green;

            default:
                return Color.yellow;



        }

    }
}