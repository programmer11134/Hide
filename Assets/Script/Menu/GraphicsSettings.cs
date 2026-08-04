using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;


public class GraphicsSettings : MonoBehaviour
{
    [Header("ui")]
    public TMP_Dropdown RESOLUTION;
    public TMP_Dropdown QUALITY;
    public Toggle fullScreenT;

    private Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions; //спрашиваем у системы какие есть разришения у пользователя

        List<string> list = new List<string>();//создаём лист который будет хронит все значения разрешений

        int curentResolutionIndex = 0; //  Индекс текущего разрешения монитора.
                                       // Именно этот пункт будет выбран в Dropdown при запуске игры.


        foreach (Resolution resolution in resolutions)
        {
            string option = resolution.width + " x " + resolution.height + " " + resolution.refreshRateRatio.value + "HZ"; // строковое значение, которая будет показыватьься в dropdown
            list.Add(option);// и уже после того как мы всё создали, оно будет переходить в лист add добавить

            if (resolution.width == Screen.currentResolution.width && resolution.height == 
                Screen.currentResolution.height && resolution.refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
            {
                curentResolutionIndex = list.Count - 1;// Запоминаем номер текущего разрешения,
                                                       // чтобы после заполнения Dropdown выбрать его автоматически. 
            }
        }// цикл который позволяет нам заполнять список который мы создали


        RESOLUTION.ClearOptions();// очищяем deopdown
        RESOLUTION.AddOptions(list);// добовляем наши переменные, которые получились в цикле 

        RESOLUTION.value = curentResolutionIndex; // Выбираем в Dropdown текущее разрешение.
                                                  // Само разрешение экрана пока НЕ меняется.
        RESOLUTION.RefreshShownValue(); // Обновить список dropdown


        QUALITY.ClearOptions();// убераем в dropdown все паеременные 

        List<string> quality = new List<string>();// создаём лист где будут храниться наши переменные low medium Higt

        foreach (string Q in QualitySettings.names)
        {
            quality.Add(Q);
        }// цикл выдаёт нам все значенгия low medium higt и добовляет в список q > quality
        QUALITY.AddOptions(quality);// добовляем в опции dropdown low medium higt

        QUALITY.value = QualitySettings.GetQualityLevel();// Получаем текущий уровень качества из Unity
                                                          // и выбираем его в Dropdown.
        QUALITY.RefreshShownValue();//то же самое обновдление списка dropdown

        LoadSettings();// Загружаем сохранённые настройки
                       // (если пользователь уже запускал игру раньше)
    }

    public void SetResolution(int resolutImdex)// 0 1 2 3
    { 
        Resolution resolution = resolutions[resolutImdex];

        Screen.SetResolution(resolution.width,resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
    }// вся переменная говорит нам что пользоватедль выбрал 2 номер и программа выполняет то что номер 2 это 1280 на 720  и вторая команда уже принимает все настройки которые она имеент ширину высоту герц и полноэкранный реджим 

    public void ApplyFullScreen()
    {
        if (fullScreenT.isOn)// равен истине то будет полноэкранный режим 
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else { Screen.fullScreenMode = FullScreenMode.Windowed; }// иначе будет оконный если false
    }

    public void SaveSettings()
    {
        Resolution resolution = resolutions[RESOLUTION.value];

        PlayerPrefs.SetInt("Width", resolution.width);
        PlayerPrefs.SetInt("Height", resolution.height);

        PlayerPrefs.SetInt("FullScreen", fullScreenT.isOn ? 1 : 0);

        PlayerPrefs.SetInt("hz", (int)resolution.refreshRateRatio.value);

        PlayerPrefs.SetInt("Quality", QUALITY.value);

        PlayerPrefs.Save();

        Screen.fullScreenMode = 
            fullScreenT.isOn ? // вопрос тру или фолс будет выбераться полный или неполный экранный режим 
            FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;// это уже применение 

        Screen.SetResolution
            (
                resolution.width,//ширина
                resolution.height,//высота
                Screen.fullScreenMode,//полн=неполн режим экрана
                resolution.refreshRateRatio// герц
                // всё это применяеться 
            );

        QualitySettings.SetQualityLevel(QUALITY.value);//под каким номер у нас стоит в дроп довне то и будеи выберать например 1 2 [3] 4 будет значит высокая графика 


    }//вся переменная позволяет сохронить на жоский диск высё то что выбрал пользователь в настройках 
    // так же после сохронения будет сразу применяться 

    void LoadSettings()
    {

        if (!PlayerPrefs.HasKey("Width"))
        { 
            return;
        }// делаем проверку если нет сохронёных то не чего не возрощяем 

        int width = PlayerPrefs.GetInt("Width");//забераем сохроненую ширину и добовляем в переменную инт

        int height = PlayerPrefs.GetInt("Height");//забераем сохроненую высоту и добовляем в переменную инт

        int hz = PlayerPrefs.GetInt("hz");//забераем сохроненую чистату  и добовляем в переменную инт

        bool isOnT = PlayerPrefs.GetInt("FullScreen") == 1;// забираем полный неполный экранный режим и смотрим если у нас 1 = 1 значит тру если 0 = 1 то значит фолс

        int quality = PlayerPrefs.GetInt("Quality"); // забираем сохроненую граффику в переменную инт 

        fullScreenT.isOn = isOnT; // работаем с галочкой есил isOnT равно тру то галочка стоит так же в другом порядке 


        QualitySettings.SetQualityLevel(quality);// ставим ту граффику которую сохронядли и берем её из инт

        QUALITY.value = quality;//  меняем в тексте значаение 
        QUALITY.RefreshShownValue();// и обновляем dropdown

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == width && resolutions[i].height == height && resolutions[i].refreshRateRatio.value == hz)
            {
                RESOLUTION.value = i;// ставим тот текст котрый получился при цмкле 1920 на 1080
                RESOLUTION.RefreshShownValue();// обновляем текст

                break;
            }
        }// цикл проверяет все значения высоты ширины и герцовки и если высота равна высоте котрая сохронядлась так же ширина и герц, то ставим в тексте dropdown,  то что у нас было в 
        // сохроненных файлах инт

        Screen.fullScreenMode =
            isOnT ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;// сдесь мы применяем галочку и полный экран или оконный 


        Screen.SetResolution(width, height, Screen.fullScreenMode, new RefreshRate { numerator = (uint)hz, denominator = 1 });// включаем те переменные ширина высота полный неполный экран и герцовка и применяем их
        // что означает крайняя переменная? Это по сути частота мы создаём новый refrach как экзепляр обьект и тд он делает так что мы получаем например 144 герца или 60 и у нас получаеться та кчто это будет выглядить так
        // 60/1 144/1 и по этому получаем частоту, проще гворя теперь вместо числа мы получаем частое еоторое потом делает нашу герцовку 
          

        

    }
}
