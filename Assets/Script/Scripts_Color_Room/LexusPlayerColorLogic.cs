using UnityEngine;
using Unity.Netcode;

public class LexusPlayerColorLogic : NetworkBehaviour
{
    public enum RoomState { Neutral, Colorized }

    [Header("Сетевая синхронизация цвета")]
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

    private PlayerMoveMain playerMove;
    private int offlinePlayerColorIndex = -1;

    private void Awake()
    {
        playerRenderer = GetComponent<Renderer>();
        playerMove = GetComponent<PlayerMoveMain>();

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
            neutralColor = new Color(0.65f, 0.65f, 0.68f);
        }

        ApplyPlayerColor(-1);
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
        }
    }

    public override void OnNetworkSpawn()
    {
        playerRenderer = GetComponent<Renderer>();
        playerMove = GetComponent<PlayerMoveMain>();
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
                UpdateRoleOnServer();
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
        // Кнопка Т для быстрой диагностики в консоли
        if (Input.GetKeyDown(KeyCode.T))
        {
            RunDiagnostics();
        }

        // Если менеджер не был найден при старте — пытаемся найти его прямо сейчас
        if (colorManager == null)
        {
            colorManager = FindObjectOfType<LexusColorManager>();

            if (colorManager != null)
            {
                // Если наконец нашли — инициализируем его свойства
                neutralColor = colorManager.neutralColor;

                if (IsSpawned)
                {
                    colorManager.activeStateIndex.OnValueChanged += OnRoomColorChanged;
                    if (IsServer)
                    {
                        UpdateRoleOnServer();
                    }
                }
                Debug.Log("[INFO] colorManager успешно найден в процессе игры!");
            }
            else
            {
                // Если все еще не нашли — выходим из кадра
                return;
            }
        }

        int roomColorIndex = colorManager.GetActiveColorIndex();

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

        if (gunObject != null)
        {
            if (roomColorIndex == -1)
            {
                gunObject.SetActive(false);
            }
            else
            {
                gunObject.SetActive(roomColorIndex != activePlayerColor);
            }
        }

        // Плавный переход цвета
        if (playerRenderer != null)
        {
            Color targetColor = GetColorByIndex(activePlayerColor);

            if (playerRenderer.material.HasProperty("_BaseColor"))
            {
                Color curColor = playerRenderer.material.GetColor("_BaseColor");
                playerRenderer.material.SetColor("_BaseColor", Color.Lerp(curColor, targetColor, Time.deltaTime * 5f));
            }
            else
            {
                playerRenderer.material.color = Color.Lerp(playerRenderer.material.color, targetColor, Time.deltaTime * 5f);
            }
        }
    }

    // Метод диагностики
    private void RunDiagnostics()
    {
        Debug.Log("=== ДИАГНОСТИКА ЦВЕТА ИГРОКА ===");

        if (colorManager == null)
        {
            Debug.LogError("ОШИБКА: colorManager равен NULL! Скрипт не может найти менеджер цвета на сцене.");
            return;
        }

        int roomColor = colorManager.GetActiveColorIndex();
        int playerColor = IsSpawned ? playerColorIndex.Value : offlinePlayerColorIndex;

        Debug.Log($"Сетевой статус (IsSpawned): {IsSpawned}");
        Debug.Log($"Индекс цвета комнаты: {roomColor} (Ожидаемый цвет: {GetColorName(roomColor)})");
        Debug.Log($"Индекс цвета игрока: {playerColor} (Ожидаемый цвет: {GetColorName(playerColor)})");

        if (playerRenderer != null)
        {
            Debug.Log($"Текущий цвет материала на рендерере: {playerRenderer.material.color}");
        }
        else
        {
            Debug.LogError("ОШИБКА: playerRenderer равен NULL на объекте!");
        }
    }

    private string GetColorName(int index)
    {
        switch (index)
        {
            case -1: return "Серый (Нейтральный)";
            case 0: return "Синий";
            case 1: return "Желтый";
            case 2: return "Красный";
            case 3: return "Зеленый";
            default: return "Неизвестно";
        }
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

        UpdateRoleOnServer();
    }

    private void OnPlayerColorChanged(int previousValue, int newValue)
    {
    }

    private void UpdateRoleOnServer()
    {
        if (!IsServer || playerMove == null || colorManager == null) return;

        int roomColor = colorManager.activeStateIndex.Value;
        int playerColor = playerColorIndex.Value;

        if (roomColor == playerColor)
        {
            playerMove.currentRole.Value = PlayerMoveMain.PlayerRole.Hider;
        }
        else
        {
            playerMove.currentRole.Value = PlayerMoveMain.PlayerRole.Seeker;
        }
    }

    private Color GetColorByIndex(int index)
    {
        switch (index)
        {
            case 0: return blue;
            case 1: return yellow;
            case 2: return red;
            case 3: return green;
            case -1: return neutralColor;
            default: return neutralColor;
        }
    }

    private int GetRandomColorIndex()
    {
        return Random.Range(0, 4);
    }

    private void ApplyPlayerColor(int colorIndex)
    {
        if (playerRenderer == null) return;

        Color col = GetColorByIndex(colorIndex);

        if (playerRenderer.material.HasProperty("_BaseColor"))
        {
            playerRenderer.material.SetColor("_BaseColor", col);
        }
        else
        {
            playerRenderer.material.color = col;
        }
    }
}