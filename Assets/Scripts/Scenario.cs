using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenario : MonoBehaviour {

	public GameObject[] props;

	public float neckLockValue = 0;
	public bool isControlled = false;


	[Header("Filter")]


	public List<CameraFilter> filters;



	[Header("Track")]

	public GameObject track;
	public float trackDuration = 10;
	public eOrientationMode trackOrientation = eOrientationMode.NODE;
	public float rotationLookAhead = 0;



	[Header("Manual")]

	public bool useVRStep = false;
	public bool canShoot = false;

	public Transform startingPosition;
	public bool useWaypoints;
	public WaypointSet waypointSet;

	public bool lockNeck = false;

	public float fwdSpeed = 10f;

	public float jumpAcceleration = 5f;
	public float maxJumpSpeed = .8f;

	public float rotSpeed = 1000f;


	public void setActive(bool isActive){

		foreach (GameObject prop in props)
			prop.SetActive (isActive);
	}
}
