using UnityEngine;
using System.Collections;

public class OnStartDisableRenderer : MonoBehaviour {

	public bool	disableChildRenderers	=	true;

	void Start () {

		//	this renderer
		Renderer rend  = GetComponent<Renderer>();
		if (rend != null) rend.enabled = false;

		//	child rederers
		if (disableChildRenderers) {
			foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
				r.enabled = false;
			}
		}		
	}

}
