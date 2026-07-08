using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : NetworkBehaviour
{
    [Header("Настройки")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -19.62f;
    public float mouseSensitivity = 200f;

    [Header("Ссылки")]
    public Camera playerCamera;
    public AudioListener audioListener; 

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
        }
        else
        {
            
            playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
        }
    }

    void Update()
    {
        
        if (!IsOwner) return;

        HandleRotation();
        HandleMovement();
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

        if (move.magnitude > 1)
        {
            move.Normalize();
        }
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}