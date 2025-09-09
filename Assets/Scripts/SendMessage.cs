using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendMessage : MonoBehaviour
{
    public void OnPointerEnter()
    {
        Debug.Log ("<color=green><b>" + "ENTER" + "</b></color>");
    }

    public void OnPointerExit()
    {
        Debug.Log ("<color=red><b>" + "EXIT" + "</b></color>");
    }

    public void OnPointerClick()
    {
        Debug.Log ("<b>" + "Click" + "</b>");
    }

}
