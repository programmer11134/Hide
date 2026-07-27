using Unity.Netcode; // Обязательно для сетевых проектов
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisableMenuCamera : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Проверяем, что это наш локальный игрок, а не чужой клиент
        if (!IsOwner) return;

        // Ищем камеру меню на сцене по тегу или имени и выключаем её
        GameObject menuCam = GameObject.FindWithTag("MainCamera");
        // Примечание: Убедитесь, что у вашей камеры меню в инспекторе стоит тег "MainCamera"
        // Или найдите её по имени: GameObject.Find("Имя_Объекта_Камеры")
       
        if (menuCam != null)
        {
            menuCam.SetActive(false);
        }
        
    }
   
    }

