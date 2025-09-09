using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTest : MonoBehaviour
{
	public Material matWhite;
	public Material matRed;
	public Material matGreen;

	Vector3 sizeBig = new Vector3 (1.5f, 1.5f, 1.5f);
	Vector3 sizeMedium = new Vector3 (1, 1, 1);
	Vector3 sizeSmall = new Vector3 (0.5f, 0.5f, 0.5f);

	MeshRenderer mr;

	private void Awake ()
	{
		mr = GetComponent<MeshRenderer> ();
	}

	public void OnPointerEnter()
	{
		mr.material = matGreen;
		transform.localScale = sizeBig;
	}

	public void OnPointerExit()
	{
		mr.material = matWhite;
		transform.localScale = sizeMedium;
	}

	public void OnPointerClick()
	{
		mr.material = matRed;
		transform.localScale = sizeSmall;
	}
	
}
