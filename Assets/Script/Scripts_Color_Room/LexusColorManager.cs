using UnityEngine;
using Unity.Netcode;

public class LexusColorManager : NetworkBehaviour
{
    public enum RoomState { Neutral, Colorized }

    [Header("Список объектов комнаты для перекраски")]
    [Tooltip("Перетащите сюда из иерархии ваш Пол, Стены, Потолок и тестовые кубы")]
    public Renderer[] roomObjects;

    [Header("Настройки цветов")]
    // Мягкий светло-серый (белый) цвет стен для обычной дневной фазы
    public Color neutralColor = new Color(0.65f, 0.65f, 0.68f);

    // Оригинальные цвета игроков из вашего скрипта
    private Color red = Color.red;
    private Color blue = Color.blue;
    private Color green = Color.green;
    private Color yellow = Color.yellow;

    [Header("Настройки таймингов")]
    public float neutralDuration = 5f;
    public float colorDuration = 10f;
    public float transitionDuration = 1.0f;

    private NetworkVariable<int> activeStateIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private RoomState currentState = RoomState.Neutral;

    // Плавный переход цвета только для материалов объектов
    private Color startColor;
    private Color targetColor;

    private float stateTimer = 0f;
    private float transitionTimer = 0f;
    private bool isTransitioning = false;
    private int lastColorIndex = -1;

    void Start()
    {
        RenderSettings.fog = false;

        // В оффлайн-тесте сразу красим объекты в нейтральный цвет
        if (!IsSpawned)
        {
            ApplySettings(neutralColor);
            targetColor = neutralColor;
            stateTimer = neutralDuration;
        }
    }

    public override void OnNetworkSpawn()
    {
        activeStateIndex.OnValueChanged += OnStateChanged;

        // При спавне в сети красим объекты в нейтральный цвет
        ApplySettings(neutralColor);
        targetColor = neutralColor;

        if (IsServer)
        {
            stateTimer = neutralDuration;
        }
    }

    public override void OnNetworkDespawn()
    {
        activeStateIndex.OnValueChanged -= OnStateChanged;
    }

    void Update()
    {
        if (IsSpawned && !IsServer) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            SwitchState();
        }

        HandleTransition();
    }

    private void HandleTransition()
    {
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float progress = transitionTimer / transitionDuration;

            Color currentAmbient = Color.Lerp(startColor, targetColor, progress);
            ApplySettings(currentAmbient);

            if (progress >= 1.0f)
            {
                isTransitioning = false;
            }
        }
    }

    private void SwitchState()
    {
        if (currentState == RoomState.Neutral)
        {
            currentState = RoomState.Colorized;
            stateTimer = colorDuration;

            int nextColorIndex = GetUniqueRandomColorIndex();

            if (IsSpawned)
            {
                activeStateIndex.Value = nextColorIndex;
            }
            else
            {
                Color nextColor = GetColorByIndex(nextColorIndex);
                TriggerTransition(nextColor);
            }
        }
        else
        {
            currentState = RoomState.Neutral;
            stateTimer = neutralDuration;

            if (IsSpawned)
            {
                activeStateIndex.Value = -1;
            }
            else
            {
                TriggerTransition(neutralColor);
            }
        }
    }

    private void OnStateChanged(int previousValue, int newValue)
    {
        if (newValue == -1)
        {
            TriggerTransition(neutralColor);
        }
        else
        {
            Color nextColor = GetColorByIndex(newValue);
            TriggerTransition(nextColor);
        }
    }

    private Color GetColorByIndex(int index)
    {
        switch (index)
        {
            case 0:
                return blue;
            case 2:
                return red;
            case 3:
                return green;
            default:
                return yellow;
        }
    }

    private int GetUniqueRandomColorIndex()
    {
        int index;
        do
        {
            index = Random.Range(0, 4);
        } while (index == lastColorIndex);

        lastColorIndex = index;
        return index;
    }

    private void TriggerTransition(Color newColor)
    {
        // Берем текущий цвет первого объекта как стартовый для плавного перехода
        if (roomObjects != null && roomObjects.Length > 0 && roomObjects[0] != null)
        {
            startColor = roomObjects[0].material.color;
        }
        else
        {
            startColor = neutralColor;
        }

        targetColor = newColor;
        transitionTimer = 0f;
        isTransitioning = true;
    }

    // Метод перекрашивает только те объекты, которые вы указали в списке
    private void ApplySettings(Color color)
    {
        if (roomObjects == null) return;

        for (int i = 0; i < roomObjects.Length; i++)
        {
            if (roomObjects[i] != null)
            {
                roomObjects[i].material.color = color;
            }
        }
    }

    // --- АГРЕССИВНОЕ СЕТЕВОЕ ВЫКЛЮЧЕНИЕ НА ВСЕХ ЭТАПАХ ВЫХОДА ---
    private void OnDisable() { ShutdownNetwork(); }
    private void OnApplicationQuit() { ShutdownNetwork(); }
    private void OnDestroy() { ShutdownNetwork(); }

    private void ShutdownNetwork()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[LexusColorManager] Сеть остановлена, порт освобожден.");
        }
    }
}