using UnityEngine;

public class LexusColorManager : MonoBehaviour
{
    public enum RoomState { Neutral, Colorized }

    [Header("Настройки цветов")]
    // Мягкий светло-серый (белый) цвет для обычной дневной фазы
    public Color neutralColor = new Color(0.65f, 0.65f, 0.68f);

    // 4 сочных цвета для охоты (строго по вашему концепту)
    public Color[] colorPool = new Color[4]
    {
        new Color(0.90f, 0.05f, 0.05f), // 1. Яркий Красный
        new Color(0.10f, 0.35f, 0.85f), // 2. Насыщенный Синий
        new Color(0.15f, 0.65f, 0.15f), // 3. Сочный Зеленый
        new Color(0.95f, 0.70f, 0.00f)  // 4. Выразительный Желтый
    };

    [Header("Настройки таймингов")]
    public float neutralDuration = 5f;      // Время обычной комнаты
    public float colorDuration = 10f;        // Время цветной комнаты
    public float transitionDuration = 1.0f;  // Скорость перехода

    [Header("Главный свет сцены")]
    public Light directionalLight;

    private RoomState currentState = RoomState.Neutral;

    // Управляем только солнцем и тенями (ambient)
    private Color startAmbient;
    private Color targetAmbient;
    private Color startSun;
    private Color targetSun;

    private float stateTimer = 0f;
    private float transitionTimer = 0f;
    private bool isTransitioning = false;
    private int lastColorIndex = -1;

    void Start()
    {
        // Выключаем туман на сцене для идеальной точности маскировки
        RenderSettings.fog = false;

        // На старте ставим мягкий дневной свет и глубокие тени
        ApplySettings(neutralColor * 0.25f, Color.white);

        targetAmbient = neutralColor * 0.25f;
        targetSun = Color.white;

        stateTimer = neutralDuration;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            SwitchState();
        }

        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float progress = transitionTimer / transitionDuration;

            Color currentAmbient = Color.Lerp(startAmbient, targetAmbient, progress);
            Color currentSun = Color.Lerp(startSun, targetSun, progress);

            ApplySettings(currentAmbient, currentSun);

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

            int randomIndex = GetRandomColorIndex();
            Color nextColor = colorPool[randomIndex];

            // Цветная фаза: солнце яркое, а тени (ambient) делаем глубокими и темными для контраста
            TriggerTransition(
                nextColor * 0.22f, // Темные сочные тени
                nextColor          // Яркое цветное солнце
            );
        }
        else
        {
            currentState = RoomState.Neutral;
            stateTimer = neutralDuration;

            // Нейтральная фаза: обычный день с контрастными глубокими тенями
            TriggerTransition(
                neutralColor * 0.25f, // Глубокие тени
                Color.white           // Обычное белое солнце
            );
        }
    }

    private int GetRandomColorIndex()
    {
        if (colorPool.Length <= 1) return 0;

        int index;
        do
        {
            index = Random.Range(0, colorPool.Length);
        } while (index == lastColorIndex);

        lastColorIndex = index;
        return index;
    }

    private void TriggerTransition(Color newAmbient, Color newSun)
    {
        startAmbient = RenderSettings.ambientLight;
        targetAmbient = newAmbient;

        startSun = directionalLight != null ? directionalLight.color : Color.white;
        targetSun = newSun;

        transitionTimer = 0f;
        isTransitioning = true;
    }

    private void ApplySettings(Color ambient, Color sunColor)
    {
        RenderSettings.ambientLight = ambient;

        if (directionalLight != null)
        {
            directionalLight.color = sunColor;
        }
    }
}