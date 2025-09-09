using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// GazeDwellAutoClick (no modifica CardboardReticlePointer):
/// - Lanza un raycast desde la camara.
/// - Si mira un objeto "interactivo", muestra un pointer y acumula tiempo (dwell).
/// - Al completar dwellTime, envia OnPointerClick al objeto mirado.
/// - No usa EventTrigger (evita errores de parametros).
public class GazeDwellAutoClick : MonoBehaviour
{
    [Header("Ray settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask interactionMask = 1 << 8; // misma capa que usa Cardboard (por defecto Layer 8)
    [Tooltip("Opcional: ademas del LayerMask, exigir que tenga este tag.")]
    [SerializeField] private bool useTagFilter = false;
    [SerializeField] private string interactableTag = "Interactable";

    [Header("Dwell (tiempo de mirada)")]
    [SerializeField] private float dwellTime = 1.2f; // segundos para auto-click
    [SerializeField] private bool clickOncePerGaze = true; // obliga a sacar la mirada para rearmar

    [Header("Pointer (visual de carga)")]
    [Tooltip("Objeto que aparece delante del target mientras acumulas dwell (p.e., un circulo).")]
    [SerializeField] private GameObject pointer;
    [Tooltip("Qué tan cerca del objeto posicionar el pointer (0= camara, 1= en el punto de impacto).")]
    [Range(0f, 1f)] [SerializeField] private float pointerT = 0.95f;
    [Tooltip("Escala base del pointer por metro de distancia.")]
    [SerializeField] private float pointerBaseScale = 0.025f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    private GameObject currentTarget;
    private float dwellElapsed;
    private bool hasClickedThisGaze;
    private Vector3 lastHitPoint;

    private void OnEnable()
    {
        if (pointer != null) pointer.SetActive(false);
    }

    private void Update()
    {
        // 1) Ray desde la camara hacia adelante
        Ray ray = new Ray(transform.position, transform.forward);
        if (drawDebugRay) Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);

        // 2) Raycast
        if (Physics.Raycast(ray, out var hit, maxDistance))
        {
            var go = hit.collider.gameObject;
            bool isInteractive = IsInteractive(go);

            if (isInteractive)
            {
                // --- transicion de target ---
                if (go != currentTarget)
                {
                    currentTarget = go;
                    dwellElapsed = 0f;
                    hasClickedThisGaze = false;
                    lastHitPoint = hit.point;
                    ShowPointer(true);
                }
                else
                {
                    lastHitPoint = hit.point;
                }

                // --- actualizar pointer (posicion/escala y "progreso") ---
                UpdatePointerVisual(transform.position, lastHitPoint);

                // --- acumular dwell y disparar click ---
                if (!hasClickedThisGaze || !clickOncePerGaze)
                {
                    dwellElapsed += Time.deltaTime;

                    if (dwellElapsed >= dwellTime)
                    {
                        // Enviar OnPointerClick sin requerir receptor (evita errores si falta método)
                        currentTarget.SendMessage("OnPointerClick", SendMessageOptions.DontRequireReceiver);

                        hasClickedThisGaze = true;

                        // Reinicio/espera segun configuracion
                        if (clickOncePerGaze)
                        {
                            // Espera salir del target para rearmar
                            dwellElapsed = 0f;
                        }
                        else
                        {
                            // Permite multiples clicks mientras sostienes la mirada (cada dwellTime)
                            dwellElapsed -= dwellTime;
                        }
                    }
                }
            }
            else
            {
                // miras algo NO interactivo
                ResetGaze();
            }
        }
        else
        {
            // no golpea nada
            ResetGaze();
        }
    }

    private bool IsInteractive(GameObject go)
    {
        if (go == null) return false;

        // Filtro por capa (LayerMask)
        bool inLayer = (interactionMask.value & (1 << go.layer)) != 0;

        if (!inLayer) return false;

        // Opcional: filtro por Tag
        if (useTagFilter && !go.CompareTag(interactableTag)) return false;

        return true;
    }

    private void UpdatePointerVisual(Vector3 camPos, Vector3 hitPoint)
    {
        if (pointer == null) return;

        // Posicion "t" entre camara y punto de impacto
        Vector3 p = Vector3.Lerp(camPos, hitPoint, Mathf.Clamp01(pointerT));
        pointer.transform.position = p;

        // Escala segun distancia y progreso (0..1)
        float distance = Vector3.Distance(camPos, hitPoint);
        float progress = Mathf.Clamp01(dwellElapsed / Mathf.Max(0.0001f, dwellTime));

        // Escala base por distancia + un ligero crecimiento con el progreso
        float scale = pointerBaseScale * distance * Mathf.Lerp(0.75f, 1.2f, progress);
        pointer.transform.localScale = Vector3.one * scale;

        if (!pointer.activeSelf) pointer.SetActive(true);
    }

    private void ShowPointer(bool show)
    {
        if (pointer == null) return;
        if (pointer.activeSelf != show) pointer.SetActive(show);
    }

    private void ResetGaze()
    {
        currentTarget = null;
        dwellElapsed = 0f;
        hasClickedThisGaze = false;
        ShowPointer(false);
    }
}
