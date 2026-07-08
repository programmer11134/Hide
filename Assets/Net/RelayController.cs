using Mono.Cecil.Cil;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayController : MonoBehaviour
{
    public TMP_Text joinCodeText;
    public TMP_InputField joinInput;
    public GameObject menuPanel;
    [Header("Камера меню")]
    public Camera menuCamera;
   

    private async void Start()
    {
        
        await UnityServices.InitializeAsync();

        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
    void FinalizeStart()
    {
        menuPanel.SetActive(false); 
        

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    
    public async void StartHostWithRelay()
    {
        try
        {
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            PlayerPrefs.SetString("FinalLobbyCode", joinCode); // вместо joinCode укажи свою переменную кода
            PlayerPrefs.Save();

            joinCodeText.text = "Код: " + joinCode;
            Debug.Log("Код игры: " + joinCode);
        

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            menuPanel.SetActive(false);
            
            FinalizeStart();
            if (NetworkManager.Singleton.IsServer)
            {
                // Вместо цифры укажите НАЗВАНИЕ вашей игровой сцены текстом
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }

        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    // Эту функцию вешаем на кнопку JOIN
    public async void StartClientWithRelay()
    {
        try
        {
            string code = joinInput.text;
            if (string.IsNullOrEmpty(code)) return;

            // Присоединяемся по коду
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            // Настраиваем транспорт клиента
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port, // Исправлено для последних версий
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            menuPanel.SetActive(false); // Прячем меню
            FinalizeStart();

        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}