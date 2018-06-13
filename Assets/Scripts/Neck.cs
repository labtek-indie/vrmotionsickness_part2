using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neck : MonoBehaviour {

	public float lockValue = .1f;
	float prevYaw = 0;

	public float relaxedYaw = 0;

	// Use this for initialization
	void Start () {
		prevYaw = transform.rotation.eulerAngles.y;
	}

	public void reset(){
		transform.localRotation = Quaternion.identity;
		relaxedYaw = 0;
		prevYaw = 0;
	}
	
	// Update is called once per frame
	void LateUpdate () {
//		transform.rotation = Quaternion.Lerp (prevYaw, transform.parent.rotation + relaxedYaw, lockValue);

		transform.RotateYTo(Mathf.LerpAngle (prevYaw, transform.parent.rotation.eulerAngles.y - relaxedYaw, lockValue));

		prevYaw = transform.rotation.eulerAngles.y;
	}
}
