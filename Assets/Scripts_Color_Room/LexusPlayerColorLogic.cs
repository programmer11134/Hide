using UnityEngine;
using Unity.Netcode;

public class LexusPlayerColorLogic : NetworkBehaviour
{
    public enum RoomState { Neutral, Colorized }

    [Header("Сетевая синхронизация цвета")]
    // Задаем стандартное сетевое значение -1 (нейтральный серый) по умолчанию
    public NetworkVariable<int> playerColorIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Ссылки на объекты")]
    public GameObject gunObject;
    public LexusColorManager colorManager;

    private Renderer playerRenderer;
    private Color neutralColor;

    private Color red = Color.red;
    private Color blue = Color.blue;
    private Color green = Color.green;
    private Color yellow = Color.yellow;

    // Переменная для оффлайн-тестирования
    private int offlinePlayerColorIndex = -1;

    // --- ЭТОТ МЕТОД КРАСИТ ИГРОКА В СЕРЫЙ ДО СТАРТА ИГРЫ (Убирает вспышку) ---
    private void Awake()
    {
        playerRenderer = GetComponent<Renderer>();
        if (colorManager == null)
        {
            colorManager = FindObjectOfType<LexusColorManager>();
        }

        if (colorManager != null)
        {
            neutralColor = colorManager.neutralColor;
        }
        else
        {
            neutralColor = new Color(0.65f, 0.65f, 0.68f); // Запасной серый цвет
        }

        ApplyPlayerColor(-1); // Принудительно делаем серой на самом первом кадре
    }

    void Start()
    {
        if (!IsSpawned)
        {
            if (colorManager == null)
            {
                colorManager = FindObjectOfType<LexusColorManager>();
            }

            if (colorManager != null)
            {
                neutralColor = colorManager.neutralColor;
            }

            ApplyPlayerColor(-1);

            if (gunObject != null)
            {
                gunObject.SetActive(false);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        playerRenderer = GetComponent<Renderer>();
        playerColorIndex.OnValueChanged += OnPlayerColorChanged;

        if (colorManager == null)
        {
            colorManager = FindObjectOfType<LexusColorManager>();
        }

        if (colorManager != null)
        {
            neutralColor = colorManager.neutralColor;
            colorManager.activeStateIndex.OnValueChanged += OnRoomColorChanged;

            if (IsServer)
            {
                int roomState = colorManager.activeStateIndex.Value;
                playerColorIndex.Value = (roomState == -1) ? -1 : GetRandomColorIndex();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        playerColorIndex.OnValueChanged -= OnPlayerColorChanged;
        if (colorManager != null)
        {
            colorManager.activeStateIndex.OnValueChanged -= OnRoomColorChanged;
        }
    }

    void Update()
    {
        if (colorManager == null || gunObject == null) return;

        int roomColorIndex = colorManager.GetActiveColorIndex();

        // Логика для оффлайн-теста (без сети)
        if (!IsSpawned)
        {
            if (roomColorIndex == -1)
            {
                offlinePlayerColorIndex = -1;
            }
            else if (roomColorIndex != -1 && offlinePlayerColorIndex == -1)
            {
                offlinePlayerColorIndex = GetRandomColorIndex();
            }
        }

        int activePlayerColor = IsSpawned ? playerColorIndex.Value : offlinePlayerColorIndex;

        // Включение/выключение пушки
        if (roomColorIndex == -1)
        {
            gunObject.SetActive(false);
        }
        else
        {
            if (roomColorIndex == activePlayerColor)
            {
                gunObject.SetActive(false); // Совпал — прячется
            }
            else
            {
                gunObject.SetActive(true); // Не совпал — охотится
            }
        }

        // Плавный переход цвета игрока
        Color targetColor = GetColorByIndex(activePlayerColor);
        playerRenderer.material.color = Color.Lerp(playerRenderer.material.color, targetColor, Time.deltaTime * 5f);
    }

    private void OnRoomColorChanged(int previousValue, int newValue)
    {
        if (!IsServer) return;

        if (newValue == -1)
        {
            playerColorIndex.Value = -1;
        }
        else
        {
            playerColorIndex.Value = GetRandomColorIndex();
        }
    }

    private void OnPlayerColorChanged(int previousValue, int newValue)
    {
        // Плавный переход цвета обрабатывается в Update
    }

    private Color GetColorByIndex(int index)
    {
        switch (index)
        {
            case 0:
                return blue;
            case 1:
                return yellow; 
            case 2:
                return red;
            case 3:
                return green;
            case -1:
                return neutralColor;
            default:
                return neutralColor; 
        }
    }

    private int GetRandomColorIndex()
    {
        return Random.Range(0, 4);
    }

    private void ApplyPlayerColor(int colorIndex)
    {
        if (playerRenderer == null) return;

        switch (colorIndex)
        {
            case 0:
                playerRenderer.material.color = Color.blue;
                break;
            case 2:
                playerRenderer.material.color = Color.red;
                break;
            case 3:
                playerRenderer.material.color = Color.green;
                break;


            case 1:
                playerRenderer.material.color = Color.yellow;
                break;

            default:

                playerRenderer.material.color = neutralColor;
                break;
        }
    }
}