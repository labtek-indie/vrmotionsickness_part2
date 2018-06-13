using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineNode : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer> ();
		if (GetComponent<Renderer>() != null) {
			foreach(MeshRenderer renderer in renderers)
				Destroy (renderer);
		}
	}

}
