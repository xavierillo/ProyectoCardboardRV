using UnityEngine;

/// ReticleDwellBridgeV2
/// - No modifica el SDK.
/// - Se suscribe a GazeManager aunque se inicialice después.
/// - Tolera micro-pérdidas del raycast (grace) para no reiniciar la barra.
[DisallowMultipleComponent]
public class ReticleDwellBridge : MonoBehaviour
{
    [Header("Gaze Manager (opcional)")]
    [SerializeField] private GazeManager gazeManager; // si no asignas, buscará Instance/FindObjectOfType

    [Header("Filtro de interacción")]
    [SerializeField] private LayerMask interactionMask;   // p.ej. Layer "Interactable"
    [SerializeField] private bool useTagFilter = false;
    [SerializeField] private string interactableTag = "Interactable";
    [SerializeField] private float maxDistance = 20f;

    [Header("Dwell (auto-click)")]
    [Min(0.1f)] [SerializeField] private float dwellTime = 1.2f;
    [SerializeField] private bool clickOncePerGaze = true;
    [Tooltip("Tolerancia sin impacto antes de cancelar (para que no parpadee).")]
    [Range(0f, 0.5f)] [SerializeField] private float lostGazeGrace = 0.12f;

    [Header("Pointer visual")]
    [SerializeField] private GameObject pointer;
    [Range(0f, 1f)] [SerializeField] private float pointerT = 0.95f;
    [SerializeField] private float pointerScalePerMeter = 0.025f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    private GameObject currentTarget;
    private Vector3 lastHitPoint;
    private float lostTimer;
    private bool hasClickedThisGaze;
    private bool subscribed;

    private void OnEnable()
    {
        TryResolveGazeManager();
        TrySubscribe();
        if (pointer) pointer.SetActive(false);
    }

    private void OnDisable()
    {
        if (gazeManager != null) gazeManager.OnGazeSelection -= OnGazeComplete;
        subscribed = false;
    }

    private void TryResolveGazeManager()
    {
        if (gazeManager != null) return;
        gazeManager = GazeManager.Instance ?? FindObjectOfType<GazeManager>(true);
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        if (gazeManager == null) return;
        gazeManager.OnGazeSelection += OnGazeComplete;
        subscribed = true;
    }

    private void Update()
    {
        if (gazeManager == null) { TryResolveGazeManager(); }
        if (!subscribed) { TrySubscribe(); }

        Vector3 origin = transform.position;
        Vector3 dir = transform.forward;
        if (drawDebugRay) Debug.DrawRay(origin, dir * maxDistance, Color.green);

        // Filtra por LayerMask directamente en el Raycast
        if (Physics.Raycast(origin, dir, out var hit, maxDistance, interactionMask))
        {
            var go = hit.collider.gameObject;
            if (!useTagFilter || go.CompareTag(interactableTag))
            {
                lostTimer = 0f;

                if (go != currentTarget)
                {
                    currentTarget = go;
                    hasClickedThisGaze = false;
                    lastHitPoint = hit.point;

                    if (gazeManager != null)
                    {
                        gazeManager.SetUpGaze(dwellTime);
                        gazeManager.StartGazeSelection(); // comienza a llenar
                    }
                }
                else
                {
                    lastHitPoint = hit.point;
                }

                UpdatePointer(origin, lastHitPoint);
                return;
            }
        }

        // Si perdemos el objetivo, espera un poco antes de cancelar (para evitar reinicios)
        if (currentTarget != null)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= lostGazeGrace)
            {
                ResetPointerAndCancel();
            }
            else
            {
                UpdatePointer(origin, lastHitPoint);
            }
        }
        else
        {
            if (pointer && pointer.activeSelf) pointer.SetActive(false);
        }
    }

    private void UpdatePointer(Vector3 camPos, Vector3 hitPoint)
    {
        if (!pointer) return;
        if (!pointer.activeSelf) pointer.SetActive(true);

        Vector3 p = Vector3.Lerp(camPos, hitPoint, pointerT);
        pointer.transform.position = p;

        float d = Vector3.Distance(camPos, hitPoint);
        pointer.transform.localScale = Vector3.one * (pointerScalePerMeter * d);

        pointer.transform.rotation = Quaternion.LookRotation(pointer.transform.position - camPos);
    }

    private void ResetPointerAndCancel()
    {
        if (pointer && pointer.activeSelf) pointer.SetActive(false);
        if (currentTarget != null && gazeManager != null) gazeManager.CancelGazeSelection();
        currentTarget = null;
        hasClickedThisGaze = false;
        lostTimer = 0f;
    }

    private void OnGazeComplete()
    {
        Debug.Log($"[ReticleDwellBridgeV2] Gaze complete on: {currentTarget?.name}");
        if (currentTarget == null) return;
        if (clickOncePerGaze && hasClickedThisGaze) return;

        currentTarget.SendMessage("OnPointerClick", SendMessageOptions.DontRequireReceiver);
        hasClickedThisGaze = true;

        ResetPointerAndCancel();
    }
}
