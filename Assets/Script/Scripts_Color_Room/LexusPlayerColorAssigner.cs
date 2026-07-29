using UnityEngine;
using Unity.Netcode;

public class LexusPlayerColorAssigner : NetworkBehaviour
{
    // Сетевая переменная хранит неизменный цвет игрока (0, 1, 2, 3)
    public NetworkVariable<int> playerColorIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [HideInInspector]
    public int offlineColorIndex = 0;

    void Start()
    {
        if (!IsSpawned)
        {
            // В оффлайне выбираем цвет ОДИН раз при запуске игры
            offlineColorIndex = Random.Range(0, 4);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // СЕТЕВАЯ РАЗДАЧА: Вызывается ОДИН раз при подключении игрока.
            // Распределяет цвета по очереди среди зашедших игроков (макс 8)
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            playerColorIndex.Value = (playerCount - 1) % 4;
        }
    }

    public int GetColorIndex()
    {
        return IsSpawned ? playerColorIndex.Value : offlineColorIndex;
    }
}