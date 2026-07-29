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
    public GameObject weaponFPS; // Оружие от 1-го лица (видит только Seeker сам у себя)
    public GameObject weaponTPS; // Оружие от 3-го лица (видят другие игроки на Seeker-е)

    [Header("Настройки Камеры")]
    public Camera playerCamera;
    public AudioListener audioListener;

    [Header("Параметры 3-го лица (Как в ААА играх)")]
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.7f, 1.6f, -2.5f);
    [SerializeField] private float cameraSmoothTime = 0.15f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 50f;
    [SerializeField] private LayerMask collisionLayers;

    [Header("Параметры движения")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -25f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float pitch = 0f;
    private float yaw = 0f;
    private Vector3 currentCameraVelocity;
    private Vector3 cameraTargetPosition;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
        currentRole.OnValueChanged += OnRoleChanged;

        if (IsOwner)
        {
            playerCamera.enabled = true;
            if (audioListener != null) audioListener.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw = transform.eulerAngles.y;
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }

        UpdateVisuals(currentRole.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentRole.OnValueChanged -= OnRoleChanged;
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole) => UpdateVisuals(newRole);

    private void UpdateVisuals(PlayerRole role)
    {
        bool isSeeker = (role == PlayerRole.Seeker);

        if (IsOwner)
        {
            // Если владелец Seeker (1-е лицо) -> показываем оружие 1-го лица. Если Hider -> оружия нет вообще
            if (weaponFPS) weaponFPS.SetActive(isSeeker);
            if (weaponTPS) weaponTPS.SetActive(false); // Владелец никогда не видит свое TPS оружие

            // В 1-м лице (Seeker) скрываем меш тела (оставляем только тени). В 3-м лице (Hider) показываем тело полностью
            ShadowCastingMode mode = isSeeker ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            SetShadowMode(playerBodyVisuals, mode);
        }
        else
        {
            // Для сторонних игроков: они видят оружие на персонаже, только если он Seeker (Ищущий)
            if (weaponFPS) weaponFPS.SetActive(false);
            if (weaponTPS) weaponTPS.SetActive(isSeeker);
        }
    }

    private void SetShadowMode(GameObject target, ShadowCastingMode mode)
    {
        if (target == null) return;
        foreach (var r in target.GetComponentsInChildren<Renderer>(true))
        {
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

    void LateUpdate()
    {
        if (!IsOwner) return;

        // ВОЗВРАЩЕНО НАЗАД:
        // Hider (Прячущийся) играет от 3-го лица
        // Seeker (Ищущий) играет от 1-го лица
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
        playerCamera.transform.position = transform.position + Vector3.up * 1.6f;
        playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void ApplyThirdPersonCamera()
    {
        Vector3 pivotPoint = transform.position + Vector3.up * shoulderOffset.y;
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos = pivotPoint + (cameraRotation * new Vector3(shoulderOffset.x, 0, shoulderOffset.z));

        RaycastHit hit;
        Vector3 dirToCamera = (targetPos - pivotPoint).normalized;
        float maxDist = Vector3.Distance(pivotPoint, targetPos);

        if (Physics.SphereCast(pivotPoint, 0.2f, dirToCamera, out hit, maxDist, collisionLayers))
        {
            targetPos = pivotPoint + dirToCamera * (hit.distance - 0.1f);
        }

        playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, targetPos, ref currentCameraVelocity, cameraSmoothTime);

        Vector3 lookAtPoint = pivotPoint + (cameraRotation * Vector3.forward * 100f);
        playerCamera.transform.LookAt(lookAtPoint);
    }
}