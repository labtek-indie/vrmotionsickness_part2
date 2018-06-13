using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eUpdateMode { NONE = 0, SCALE, ALPHA}

public class CameraFilter : MonoBehaviour {

	[SerializeField] eUpdateMode updateMode;

	[SerializeField] float minSelfValue = .2f;
	[SerializeField] float maxSelfValue = .5f;

	MeshRenderer meshRenderer;

	delegate void UpdateValueF(float value);
	UpdateValueF updateValueF;

	bool initialized;

	void init(){
		if (!initialized) {
			meshRenderer = GetComponent<MeshRenderer> ();

			switch (updateMode) {
			case eUpdateMode.NONE:
				updateValueF = doNothing;
				break;
			case eUpdateMode.SCALE:
				updateValueF = updateScale;
				break;
			case eUpdateMode.ALPHA:
				if (meshRenderer != null)
					updateValueF = updateAlpha;
				else
					updateValueF = doNothing;
				break;
			default:
				updateValueF = doNothing;
				break;
			}

			initialized = true;
		}
	}

	public void updateValue(float value){
		init ();
		updateValueF (value);
	}

	void doNothing(float value){

	}

	void updateScale(float value){
		transform.ScaleLocalTo (Mathf.Lerp (maxSelfValue, minSelfValue, value));
	}
	void updateAlpha(float value){
		meshRenderer.material.setAlpha (Mathf.Lerp (minSelfValue, maxSelfValue, value));
	}
}
