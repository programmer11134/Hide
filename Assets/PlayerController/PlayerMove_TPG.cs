using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove_TPG : NetworkBehaviour
{
    public enum PlayerRole { Seeker, Hider }

    [Header("Настройки Ролей")]
    // NetworkVariable синхронизирует состояние роли между всеми игроками
    public NetworkVariable<PlayerRole> currentRole = new NetworkVariable<PlayerRole>(
        PlayerRole.Hider,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server // Только сервер может менять роль
    );


    [Header("Настройки движения")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -19.62f;
    public float mouseSensitivity = 200f;

    [Header("Настройки Камеры")]
    public Camera playerCamera;
    public AudioListener audioListener;

    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 0.8f, 0.2f);
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 1.5f, -0.0f);
    [SerializeField] private float thirdPersonSlerpSpeed = 10f; // Плавность камеры

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera.enabled = true;
            if (audioListener != null) audioListener.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Подписываемся на обновление роли, чтобы сразу сменить камеру
            currentRole.OnValueChanged += OnRoleChanged;
            UpdateCameraPosition(currentRole.Value);
        }
        else
        {
            playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            currentRole.OnValueChanged -= OnRoleChanged;
        }
    }

    // Вызывается автоматически при изменении роли на сервере
    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        UpdateCameraPosition(newRole);
    }

    private void UpdateCameraPosition(PlayerRole role)
    {
        if (role == PlayerRole.Seeker)
        {
            // Настройки для 1-го лица
            playerCamera.transform.localPosition = firstPersonOffset;
        }
        else
        {
            // Настройки для 3-го лица (начальная позиция)
            playerCamera.transform.localPosition = thirdPersonOffset;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleRotation();
        HandleMovement();

        // Для 3-го лица можно добавить небольшое сглаживание или проверку препятствий (Raycast)
        if (currentRole.Value == PlayerRole.Hider)
        {
            ApplyThirdPersonCameraLogic();
        }
    }

    void HandleRotation()
    {
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -85f, 85f);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        float rotationY = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * rotationY);
    }

    void HandleMovement()
    {
        if (controller == null) controller = GetComponent<CharacterController>();

        bool isGrounded = controller.isGrounded;
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }

        Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");

        if (move.magnitude > 1) move.Normalize();

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void ApplyThirdPersonCameraLogic()
    {
        
        Vector3 targetPos = thirdPersonOffset;
        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPos, Time.deltaTime * thirdPersonSlerpSpeed);
    }

    // Пример функции для смены роли (должен вызывать сервер, например в GameManager)
    [ServerRpc] // Это пометка, что метод должен работать на сервере
    public void ChangeRoleServerRpc(PlayerRole newRole)
    {
        currentRole.Value = newRole;
    }
}