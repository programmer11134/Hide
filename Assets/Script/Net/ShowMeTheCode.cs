using UnityEngine;
using TMPro;

public class ShowMeTheCode : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI myText = GetComponent<TextMeshProUGUI>();
        // Просто забираем код из памяти устройства
        string code = PlayerPrefs.GetString("FinalLobbyCode", "Создание...");

        if (myText != null)
        {
            myText.text = "Код лобби: " + code;
        }
    }
}
