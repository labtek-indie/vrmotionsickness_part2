using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YawLock : MonoBehaviour {

	[SerializeField] Transform reference;

	// Update is called once per frame
	void Update () {
		transform.RotateLocalYTo (-reference.rotation.eulerAngles.y);
	}
}
