using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerRelayToParent : MonoBehaviour
{
    void OnPointerEnter(){ SendMessageUpwards("OnPointerEnter", SendMessageOptions.DontRequireReceiver); }
    void OnPointerExit() { SendMessageUpwards("OnPointerExit",  SendMessageOptions.DontRequireReceiver); }
    void OnPointerClick(){ SendMessageUpwards("OnPointerClick", SendMessageOptions.DontRequireReceiver); }
}
