using UnityEngine;
using UnityEngine.EventSystems; // Para IPointerClickHandler (opcional)

public class CardboardReticleTapBridge : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Camara desde la que se lanzara el raycast (normalmente la del Player).")]
    public Camera cam;

    [Tooltip("Capa(s) de interaccion. Marca aqui 'Interactiva' (igual que en el Reticle).")]
    public LayerMask interactionMask;

    [Header("Raycast")]
    [Tooltip("Distancia maxima de interaccion")]
    public float maxDistance = 25f;

    void Reset()
    {
        cam = Camera.main;
    }

    /// <summary>
    /// Llama a este metodo desde el OnTap del CameraController.
    /// </summary>
    public void PerformTap()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) { Debug.LogWarning("[TapBridge] No hay camara asignada."); return; }

        // Ray desde el centro de la camara (hacia adelante)
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionMask))
        {
            GameObject go = hit.transform.gameObject;

            // Ruta 1: patron usado en HelloCardboard (SendMessage a OnPointerClick)
            go.SendMessage("OnPointerClick", SendMessageOptions.DontRequireReceiver);

            // Ruta 2 (opcional): si usas IPointerClickHandler (UI-like handlers)
            // Ejecuta en este objeto y/o en padres:
            var handlers = go.GetComponentsInParent<MonoBehaviour>(true);
            foreach (var h in handlers)
            {
                if (h is IPointerClickHandler ip)
                {
                    var ev = new PointerEventData(EventSystem.current);
                    ip.OnPointerClick(ev);
                }
            }

            // Debug opcional:
            // Debug.Log($"[TapBridge] TAP en: {go.name}");
        }
        else
        {
            // Debug opcional:
            // Debug.Log("[TapBridge] No hay objetivo bajo la reticula.");
        }
    }
}
