using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

	[SerializeField] Target target;

	void OnEnable(){
		if (target != null)
			target.gameObject.SetActive (true);
	}

	void OnDisable(){
		if (target != null)
			target.gameObject.SetActive (false);
	}

	public bool isPassable(){
		if (target != null && !target.IsShot)
			return false;
		return true;
	}
}
