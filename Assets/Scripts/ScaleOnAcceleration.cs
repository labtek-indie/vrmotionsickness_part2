using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOnAcceleration : MonoBehaviour {

	public Transform reference;

	float prevValue;
	float prevValueChange;

	float prevSpeed;
	float prevAcc;
	Vector3 prevPosition;
	float prevYRotation;

	float minScale = .2f;
	float maxScale = .6f;

	float minAcc = .2f;
	float maxAcc = .6f;

	float minRSpeed = .1f;
	float maxRSpeed  = 6f;

	public float speed;
	public float acceleration;
	public float rSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {

		speed = Vector3.Distance (reference.position, prevPosition) / Time.deltaTime;
		acceleration = Mathf.Abs (speed - prevSpeed) / Time.deltaTime;

		float accelerationScale = Mathf.InverseLerp(minAcc, maxAcc, acceleration);

		rSpeed = Mathf.Abs (reference.rotation.eulerAngles.y - prevYRotation) / Time.deltaTime;

		float rotationalScale = Mathf.InverseLerp(minRSpeed, maxRSpeed, rSpeed);


		float value = (accelerationScale * .3f) + (rotationalScale * .7f);

		float valueChange = value - prevValue;

		if (valueChange < 0) {
			valueChange = prevValueChange - .01f;
			valueChange = prevValueChange - .001f;
		}

		value = prevValue + (valueChange * .1f);


		updateScale (value);

		prevSpeed = speed;
		prevPosition = reference.position;
		prevYRotation = reference.rotation.eulerAngles.y;
		prevAcc = acceleration;

		prevValue = value;
		prevValueChange = valueChange;
	}

	void updateScale(float value){
		transform.ScaleLocalTo (Mathf.Lerp (maxScale, minScale, value));
	}
}
