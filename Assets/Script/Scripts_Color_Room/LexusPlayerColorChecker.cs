using UnityEngine;

public class LexusPlayerColorChecker : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    public LexusPlayerColorAssigner colorAssigner;
    public LexusColorManager colorManager;
    public GameObject gunObject;

    private Renderer playerRenderer;

    private Color red = Color.red;
    private Color blue = Color.blue;
    private Color green = Color.green;
    private Color yellow = Color.yellow;
    private Color neutralColor;

    void Start()
    {
        playerRenderer = GetComponent<Renderer>();

        if (colorAssigner == null) colorAssigner = GetComponent<LexusPlayerColorAssigner>();
        if (colorManager == null) colorManager = FindObjectOfType<LexusColorManager>();

        if (colorManager != null)
        {
            neutralColor = colorManager.neutralColor;
        }

        if (gunObject != null)
        {
            gunObject.SetActive(false);
        }
    }

    void Update()
    {
        if (colorAssigner == null || colorManager == null || gunObject == null) return;

        int roomColorIndex = colorManager.GetActiveColorIndex();
        int playerSecretColor = colorAssigner.GetColorIndex();

        Color targetColor;

        if (roomColorIndex == -1)
        {
            // ФАЗА 1: Серый день. Игрок маскируется под серый
            targetColor = neutralColor;
            gunObject.SetActive(false); // Пушки нет
        }
        else
        {
            // ФАЗА 2: Охота. Игрок плавно показывает свой постоянный цвет!
            targetColor = GetColorByIndex(playerSecretColor);

            // Проверяем совпадение постоянного цвета игрока и текущего цвета комнаты
            if (roomColorIndex == playerSecretColor)
            {
                gunObject.SetActive(false); // Совпал — прячется (без пушки)
            }
            else
            {
                gunObject.SetActive(true); // Не совпал — охотится (пушка включена)
            }
        }

        // Плавный переход цвета материала пилюли
        playerRenderer.material.color = Color.Lerp(playerRenderer.material.color, targetColor, Time.deltaTime * 5f);
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
            case -1:
                return neutralColor;
            default:
                return yellow;
        }
    }
}