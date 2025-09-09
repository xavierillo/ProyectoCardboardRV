using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // Nuevo Input System
#endif

public class CameraController : MonoBehaviour
{
    [Header("Look (solo Editor/PC)")]
    [SerializeField] bool holdRightMouseToLook = true;    // RMB para mirar
    [Range(0.05f, 0.5f)] [SerializeField] float sensitivity = 0.15f;
    [SerializeField] float maxVerticalAngle = 80f;

    [Header("Tap")]
    [SerializeField] bool simulateTapWithLeftMouse = true; // LMB simula tap
    public UnityEvent OnTap; // Asigna aquí qué hacer cuando se detecta TAP

    float yaw, pitch;

    void Start()
    {
        var e = transform.localEulerAngles;
        yaw = e.y; pitch = e.x;
    }

    void Update()
    {
    #if UNITY_EDITOR || UNITY_STANDALONE
        // --- Mirar (drag) con el mouse en PC/Editor ---
        bool dragging =
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            (Mouse.current != null && (holdRightMouseToLook ? Mouse.current.rightButton.isPressed
                                                            : Mouse.current.leftButton.isPressed));
        #else
            (holdRightMouseToLook ? Input.GetMouseButton(1) : Input.GetMouseButton(0));
        #endif

        if (dragging)
        {
            float dx, dy;
            #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Vector2 d = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            dx = d.x; dy = d.y; // píxeles por frame
            #else
            dx = Input.GetAxis("Mouse X") * 100f;
            dy = Input.GetAxis("Mouse Y") * 100f;
            #endif

            yaw   += dx * sensitivity;
            pitch  = Mathf.Clamp(pitch - dy * sensitivity, -maxVerticalAngle, maxVerticalAngle);
            transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // --- Simular TAP con click izquierdo ---
        if (simulateTapWithLeftMouse)
        {
            bool leftClicked =
            #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
            #else
                Input.GetMouseButtonDown(0);
            #endif

            if (leftClicked) PerformTap();
        }
    #endif

    #if UNITY_ANDROID && !UNITY_EDITOR
        // --- TAP real en Android ---
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame) PerformTap();
        #else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) PerformTap();
        #endif
    #endif
    }

    void PerformTap()
    {
        OnTap?.Invoke();
        // Debug.Log("TAP!");
    }
}
