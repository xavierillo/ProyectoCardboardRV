using UnityEngine;

/// Dibuja un rayo de depuración desde la cámara y cambia de color según el objetivo.
/// Verde  = no golpea nada
/// Amarillo = golpea algo NO interactivo
/// Rojo = golpea un objeto INTERACTIVO (capa InteractionMask)
public class GazeRayDebug : MonoBehaviour
{
    [Header("Ray settings")]
    [SerializeField] private float maxDistance = 80f;
    [SerializeField] private LayerMask interactionMask; // asigna la capa "Interactiva"

    [Header("Colors")]
    [SerializeField] private Color colorNoHit = Color.green;
    [SerializeField] private Color colorHitNonInteractive = Color.yellow;
    [SerializeField] private Color colorHitInteractive = Color.red;
    [SerializeField] private float drawDuration = 0f; // 0 = dura un frame

    private void Update()
    {
        var ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out var hit, maxDistance))
        {
            // ¿El objeto golpeado está en la capa "Interactiva"?
            bool isInteractive = (interactionMask.value & (1 << hit.collider.gameObject.layer)) != 0;
            var debugColor = isInteractive ? colorHitInteractive : colorHitNonInteractive;
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, debugColor, drawDuration);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, colorNoHit, drawDuration);
        }
    }
}
