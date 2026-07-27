using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMoveMain : NetworkBehaviour
{
    public enum PlayerRole { Seeker, Hider }

    [Header("Настройки Ролей")]
    public NetworkVariable<PlayerRole> currentRole = new NetworkVariable<PlayerRole>(
        PlayerRole.Hider,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Ссылки на объекты")]
    public GameObject playerBodyVisuals;
    public GameObject weaponFPS;
    public GameObject weaponTPS;

    [Header("Настройки Камеры")]
    public Camera playerCamera;
    public AudioListener audioListener;

    [Header("Параметры 3-го лица (Как в ААА играх)")]
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.7f, 1.6f, -2.5f); // Смещение: вправо, вверх, назад
    [SerializeField] private float cameraSmoothTime = 0.15f; // Плавность камеры
    [SerializeField] private float minPitch = -40f; // Ограничение взгляда вниз
    [SerializeField] private float maxPitch = 50f;  // Ограничение взгляда вверх
    [SerializeField] private LayerMask collisionLayers;

    [Header("Параметры движения")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -25f; // Усиленная гравитация для реализма
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float pitch = 0f; // Вращение камеры вверх/вниз
    private float yaw = 0f;   // Вращение игрока влево/вправо
    private Vector3 currentCameraVelocity;
    private Vector3 cameraTargetPosition;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();

        if (IsOwner)
        {
            playerCamera.enabled = true;
            if (audioListener != null) audioListener.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Начальные углы
            yaw = transform.eulerAngles.y;

            currentRole.OnValueChanged += OnRoleChanged;
            UpdateVisuals(currentRole.Value);
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
            if (weaponFPS) weaponFPS.SetActive(false);
            if (weaponTPS) weaponTPS.SetActive(true);
        }
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole) => UpdateVisuals(newRole);

    private void UpdateVisuals(PlayerRole role)
    {
        if (!IsOwner) return;

        bool isFirstPerson = (role == PlayerRole.Seeker);

        if (weaponFPS) weaponFPS.SetActive(isFirstPerson);
        if (weaponTPS) weaponTPS.SetActive(!isFirstPerson);

        // В 3-м лице ВСЕГДА показываем тело полностью
        ShadowCastingMode mode = isFirstPerson ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
        SetShadowMode(playerBodyVisuals, mode);
    }

    private void SetShadowMode(GameObject target, ShadowCastingMode mode)
    {
        if (target == null) return;
        foreach (var r in target.GetComponentsInChildren<Renderer>(true))
        {
            // Руки для 1-го лица всегда видны
            if (weaponFPS != null && r.transform.IsChildOf(weaponFPS.transform))
                r.shadowCastingMode = ShadowCastingMode.On;
            else
                r.shadowCastingMode = mode;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleInput();
        HandleMovement();
    }

    void LateUpdate() // Камера всегда в LateUpdate для отсутствия дрожания
    {
        if (!IsOwner) return;

        if (currentRole.Value == PlayerRole.Hider)
            ApplyThirdPersonCamera();
        else
            ApplyFirstPersonCamera();
    }

    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void HandleMovement()
    {
        // Вращаем персонажа по горизонтали (yaw)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        bool isGrounded = controller.isGrounded;
        if (isGrounded && moveDirection.y < 0) moveDirection.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        if (move.magnitude > 1) move.Normalize();

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    void ApplyFirstPersonCamera()
    {
        // Просто привязываем к голове
        playerCamera.transform.position = transform.position + Vector3.up * 1.6f;
        playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void ApplyThirdPersonCamera()
    {
        // 1. Рассчитываем идеальную позицию камеры
        // Точка вращения (Pivot) - это центр игрока + смещение по высоте
        Vector3 pivotPoint = transform.position + Vector3.up * shoulderOffset.y;

        // Поворот камеры (объединяем горизонтальный yaw и вертикальный pitch)
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        // Смещение камеры назад и вбок относительно поворота
        Vector3 targetPos = pivotPoint + (cameraRotation * new Vector3(shoulderOffset.x, 0, shoulderOffset.z));

        // 2. Проверка коллизий (чтобы камера не заходила в стены)
        RaycastHit hit;
        Vector3 dirToCamera = (targetPos - pivotPoint).normalized;
        float maxDist = Vector3.Distance(pivotPoint, targetPos);

        if (Physics.SphereCast(pivotPoint, 0.2f, dirToCamera, out hit, maxDist, collisionLayers))
        {
            targetPos = pivotPoint + dirToCamera * (hit.distance - 0.1f);
        }

        // 3. Плавное движение камеры к цели
        playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, targetPos, ref currentCameraVelocity, cameraSmoothTime);

        // 4. Поворот камеры: она должна смотреть в точку перед игроком (прицел)
        // Рассчитываем точку, куда игрок "целится"
        Vector3 lookAtPoint = pivotPoint + (cameraRotation * Vector3.forward * 100f);
        playerCamera.transform.LookAt(lookAtPoint);
    }
}