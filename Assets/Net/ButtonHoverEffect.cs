using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    [Tooltip("На сколько увеличится кнопка. 1.1 — это увеличение на 10%")]
    public float hoverScaleMultiplier = 1.1f;

    void Start()
    {
        // Запоминаем начальный размер кнопки при старте игры
        originalScale = transform.localScale;
    }

    // Метод срабатывает автоматически, когда курсор мыши заходит на кнопку
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScaleMultiplier;
    }

    // Метод срабатывает автоматически, когда курсор мыши уходит с кнопки
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}
