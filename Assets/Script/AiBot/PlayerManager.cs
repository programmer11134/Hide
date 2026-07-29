using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;
    public void Awake()
    {
        instance = this;
    }
    #endregion
    public GameObject player;
}
