using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFilterManager : MonoBehaviour {

	public Transform reference;

	float prevValue;
	float prevValueChange;

	float prevSpeed;
	float prevAcc;
	Vector3 prevPosition;
	float prevYRotation;
	float prevHeadYRotation;

	float minSpeed = 1f;
	float maxSpeed = 3f;

	float minAcc = .2f;
	float maxAcc = .6f;

	float minRSpeed = 0;
	float maxRSpeed = 4f;

	float minHeadRSpeed = 3f;
	float maxHeadRSpeed = 20f;


	public float speed;
	public float acceleration;
	public float rSpeed;
	public float headRSpeed;


	[SerializeField] List<Transform> _singleEyeFilters;

	[SerializeField] List<CameraFilter> _filters;


	public void init(){
		
		if (_singleEyeFilters != null && _singleEyeFilters.Count > 0) {
			
			List<GameObject> filters = new List<GameObject> ();

			List<Camera> eyes = Camera.main.GetComponentsInChildren<Camera> ().ToList ();
			eyes.Remove (Camera.main);

			int leftEyeLayer = LayerMask.NameToLayer ("LeftEye");

			bool toLeftEye = true;

			if (eyes != null && eyes.Count == 2) {
			
				eyes [0].cullingMask &= ~(1 << leftEyeLayer + 1);
				eyes [1].cullingMask &= ~(1 << leftEyeLayer);

				foreach (Transform filter in _singleEyeFilters) {
					Vector3 origPosition = filter.localPosition;
					Quaternion origRotation = filter.localRotation;

					filter.SetParent (eyes[toLeftEye ? 0 : 1].transform);

					filter.localPosition = origPosition;
					filter.localRotation = origRotation;

					toLeftEye = !toLeftEye;
				}
			}
		}
	}


	public void reset(List<CameraFilter> filters){
		
		foreach (CameraFilter filter in _filters) {
			filter.gameObject.SetActive (false);
		}


		if (filters.Count == 0)
			gameObject.SetActive (false);
		
		else {
			gameObject.SetActive (true);

			foreach (CameraFilter filter in filters) {
				if (_filters.Contains (filter))
					filter.gameObject.SetActive (true);
			}
		}

	}


	// Update is called once per frame
	void LateUpdate () {

		speed = Vector3.Distance (reference.position, prevPosition) / Time.deltaTime;

		float speedScale = Mathf.InverseLerp (minSpeed, maxSpeed, speed);



		acceleration = Mathf.Abs (speed - prevSpeed) / Time.deltaTime;

		float accelerationScale = Mathf.InverseLerp(minAcc, maxAcc, acceleration);

		

		rSpeed = Mathf.Abs (reference.rotation.eulerAngles.y - prevYRotation) / Time.deltaTime;

		float rotationalScale = Mathf.InverseLerp (minRSpeed, maxRSpeed, rSpeed);



		headRSpeed = Mathf.Abs (transform.rotation.eulerAngles.y - prevHeadYRotation) / Time.deltaTime;

		float headRotationalScale = Mathf.InverseLerp (minHeadRSpeed, maxHeadRSpeed, headRSpeed);



		float value = (speedScale * .1f) + (accelerationScale * .6f) + (headRotationalScale * .8f) + (rotationalScale * .8f);
		value = Mathf.Min (1, value);

//		print (value);

		float valueChange = value - prevValue;

		if (valueChange > 0) {
			valueChange = Mathf.Min(.03f, valueChange);
		}
		else if (valueChange < 0) {
			valueChange = prevValueChange - .003f;
		}

		value = prevValue + (valueChange * .3f);

		if (_filters != null) {
			foreach (CameraFilter filter in _filters) {
				if(filter.gameObject.activeInHierarchy)
					filter.updateValue (value);
			}
		}



		prevSpeed = speed;
		prevPosition = reference.position;
		prevYRotation = reference.rotation.eulerAngles.y;
		prevHeadYRotation = transform.rotation.eulerAngles.y;
		prevAcc = acceleration;

		prevValue = value;
		prevValueChange = valueChange;
	}


}
