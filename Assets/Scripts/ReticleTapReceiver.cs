using UnityEngine;
using System.Collections;

public class ReticleTapReceiver : MonoBehaviour
{
    bool _deactivating;

    public void OnPointerEnter() { /* opcional: highlight */ }
    public void OnPointerExit()  { /* opcional: quitar highlight */ }

    public void OnPointerClick()
    {
        Debug.Log("ReticleTapReceiver: OnPointerClick - Desactivando objeto...");
        if (_deactivating) return;
        _deactivating = true;

        // 1) Avisar salida a la reticula antes de tocar nada.
        gameObject.SendMessage("OnPointerExit", SendMessageOptions.DontRequireReceiver);

        // 2) Volver NO-interactivo de inmediato.
        //SetInteractive(false);                     // desactiva colliders + cambia layer
       // HideRenderers(true);                       // opcional: oculto visualmente

        // 3) Destruir al final del frame (ya sin referencias activas).
        //StartCoroutine(DestroyEndOfFrame());
        // Alternativa simple: SetActive(false) y NO destruir.
         gameObject.SetActive(false);
    }

    void SetInteractive(bool state)
    {
        // Deshabilita TODOS los colliders del objeto e hijos
        foreach (var col in GetComponentsInChildren<Collider>(true))
            col.enabled = state;

        // Cambia layer a una NO incluida en el ReticleInteractionLayerMask (p.ej., Default)
        SetLayerRecursive(transform, LayerMask.NameToLayer("Default"));
    }

    void HideRenderers(bool hide)
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = !hide;
    }

    IEnumerator DestroyEndOfFrame()
    {
        // Espera un frame (o WaitForEndOfFrame) para que CardboardReticlePointer
        // haga su Update y suelte cualquier referencia previa.
        yield return null; // o: yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform c in t) SetLayerRecursive(c, layer);
    }
}
