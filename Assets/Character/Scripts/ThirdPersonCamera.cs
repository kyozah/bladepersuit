using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player

    [Header("Camera Position - FIXED")]
    [Tooltip("Khoảng cách từ player (FIXED, không thay đổi)")]
    public float distance = 5f;

    [Tooltip("Độ cao từ player (FIXED, không thay đổi)")]
    public float height = 2f;

    [Tooltip("Offset sang trái/phải")]
    public float sideOffset = 0f;

    [Header("Camera Rotation - MOUSE ONLY")]
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.3f;
    public float collisionBuffer = 0.2f;

    private float currentYaw = 0f;
    private float currentPitch = 15f;

    // Input System
    private PlayerInputActions inputActions;
    private Vector2 lookInput;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("❌ Target not assigned!");
            return;
        }

        // Khởi tạo góc camera theo hướng player
        currentYaw = target.eulerAngles.y;

        // Khóa cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        // Read Look input
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // Apply sensitivity
        float mouseX = lookInput.x * mouseSensitivity * 0.02f;
        float mouseY = lookInput.y * mouseSensitivity * 0.02f;

        // ✅ CHỈ cập nhật rotation từ chuột
        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // ESC để mở cursor
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Click để khóa lại
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void UpdateCameraPosition()
    {
        // ✅ Tính rotation từ mouse input
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // ✅ FIXED focus point - KHÔNG smooth, KHÔNG follow bobbing
        Vector3 focusPoint = target.position + Vector3.up * height;

        // ✅ Tính camera position - FIXED distance
        Vector3 direction = rotation * Vector3.back;
        Vector3 rightOffset = rotation * Vector3.right * sideOffset;

        Vector3 desiredPosition = focusPoint + direction * distance + rightOffset;

        // Check collision
        Vector3 finalPosition = CheckCameraCollision(focusPoint, desiredPosition);

        // ✅ TRỰC TIẾP set position - KHÔNG smooth, KHÔNG lerp
        transform.position = finalPosition;

        // ✅ Camera nhìn vào focus point
        transform.LookAt(focusPoint);
    }

    Vector3 CheckCameraCollision(Vector3 fromPos, Vector3 toPos)
    {
        Vector3 direction = toPos - fromPos;
        float desiredDistance = direction.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(
            fromPos,
            collisionRadius,
            direction.normalized,
            out hit,
            desiredDistance,
            collisionLayers
        ))
        {
            float safeDistance = hit.distance - collisionRadius - collisionBuffer;
            return fromPos + direction.normalized * Mathf.Max(safeDistance, 0.5f);
        }

        return toPos;
    }

    public float GetCameraYaw()
    {
        return currentYaw;
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Draw focus point
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position + Vector3.up * height, 0.3f);

        // Draw camera line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position + Vector3.up * height);
    }
}