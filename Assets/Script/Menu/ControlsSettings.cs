using System;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettings : MonoBehaviour
{
    
    public Slider SensRange;
    [Range(0f, 10f)]
    public float Sens;

    public Text TextMeshPro;

    private void Start()
    {
        Sens = PlayerPrefs.GetFloat("Sensivity", 2.0f); // ѕровер€ем есть ли сохронЄнна€ сенса если нет, то ставим стандарт 2

        SensRange.value = Sens;// »змен€ем слайдер по тому числу которое получаем при выбраном сохронение если нет то значение 2

        TextMeshPro.text = Sens.ToString("F1");// “екст тоже самое делает что и слайдер измен€етьс€ в зависимости от значени€ Sens

    }

    public void SliderSvipe(float value)
    {
        Sens = SensRange.value;// —енса = чему равен ползунок 

        TextMeshPro.text = Sens.ToString("F1");//»зменение текста 
    }// ¬ешаем на слайдер, при изменение слайдера будет срабатывать код 

    public void SaveContols()
    {
        PlayerPrefs.SetFloat("Sensivity", Sens);
        PlayerPrefs.Save();
    }
}
