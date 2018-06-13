using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStep;

public class PlayerController : MonoBehaviour {

	[SerializeField] Menu menu;
	[SerializeField] Neck neck;
	[SerializeField] CameraFilterManager filterManager;

	[SerializeField] StepDetector stepDetector;
	WIPController WIPController;
	Rigidbody VRStepBody;



	[SerializeField] Transform scenarioRoot;
	[SerializeField] Scenario[] scenarios;
	[SerializeField] Scenario scenario;

	string[] scenarioKeys = new string[]{"1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "t", "y", "u", "i", "o", "p", "g", "h", "j", "k", "l"};

	SplineController splineController;

	[SerializeField] GameObject[] trackAlts;
	[SerializeField] WaypointSet[] waypointAlts;



	[Header("Manual Controller")]

	CharacterController characterController;

	[SerializeField] float runMultiplier = 2f;

	bool isStartingJump = false;
	float crntYSpeed = 0;

	[SerializeField] float gravity = 40f;

	[SerializeField] float rotSpeed = 1000f;
	Vector3 moveDirection = new Vector3 ();

	Vector2 screenCenter = new Vector2();

	[SerializeField] GameObject crosshair;


	WaypointSet crntWaypointSet;

	void OnEnable(){
		AppMaster.OnAppStep += step;
		AppMaster.OnAppPause += pause;
		AppMaster.OnScenarioSelect += reset;
	}

	void OnDisable(){
		AppMaster.OnAppStep -= step;
		AppMaster.OnAppPause -= pause;
		AppMaster.OnScenarioSelect -= reset;
	}

	// Use this for initialization
	void Start () {
		Invoke ("init", .1f);
	}

	void init(){
		scenarios = scenarioRoot.GetComponentsInChildren<Scenario> ();

		menu.init (scenarios);
		menu.gameObject.SetActive (false);

		filterManager.init ();

		splineController = GetComponent<SplineController> ();

		characterController = GetComponent<CharacterController> ();


		screenCenter.x = Screen.width / 2f;
		screenCenter.y = Screen.height / 2f;


		WIPController = GetComponent<WIPController> ();
		VRStepBody = GetComponent<Rigidbody> ();

		stepDetector.enabled = false;
		WIPController.enabled = false;


		AppMaster.i.DoScenarioSelect (0);
	}


	void reset(int scenarioIdx = -1){

		if (scenario != null)
			scenario.setActive (false);

		if (crntWaypointSet != null)
			crntWaypointSet.gameObject.SetActive (false);


		if(scenarioIdx >= 0 && scenarioIdx < scenarios.Length){
			scenario = scenarios[scenarioIdx];

			print (scenario.name);

			scenario.setActive (true);

			neck.lockValue = scenario.neckLockValue;
			filterManager.reset (scenario.filters);

			crosshair.SetActive (scenario.canShoot);

			VRStepBody.isKinematic = !scenario.isControlled;

			// if on rail
			if (!scenario.isControlled) {
				
				splineController.Duration = scenario.trackDuration;
				splineController.OrientationMode = scenario.trackOrientation;
				splineController.rotationLookAhead = scenario.rotationLookAhead;

				neck.reset ();


				// if track is supplied, initialize with it
				if (scenario.track != null)
					splineController.init (scenario.track);
				// otherwise, randomize
				else
					splineController.init (trackAlts [Random.Range (0, trackAlts.Length - 1)]);
				
			} 
			// else if controlled
			else {
				// stop spline controller
				splineController.stop ();

				if (scenario.startingPosition != null)
					transform.TransferTo (scenario.startingPosition);

				if (scenario.useWaypoints) {
					
					crntWaypointSet = scenario.waypointSet;
					
					if (crntWaypointSet == null)
						crntWaypointSet = waypointAlts [Random.Range (0, waypointAlts.Length - 1)];
					
					crntWaypointSet.reset ();
				}

				// if not using VRStep
				if (!scenario.useVRStep) {

					// disable VRStep
					stepDetector.enabled = false;
					WIPController.enabled = false;
				}

				// if using VRStep
				else {
					// enable VRStep
					stepDetector.enabled = true;
					WIPController.enabled = true;
				}
			}
		}
		else
			scenario = null;
	}

	void pause (bool isPaused){
		menu.setActive (isPaused);

		if(!isPaused)
			VRStepBody.isKinematic = !scenario.isControlled;
		else
			VRStepBody.isKinematic = true;
	}
	
	// Update is called once per frame
	void step () {


		for (int i = 0; i < scenarioKeys.Length; i++) {
			if (Input.GetKeyDown (scenarioKeys [i])) {
				reset (i);
				break;
			}
		}


		if (scenario != null) {
			if (scenario.isControlled) {

				// if not using VRStep
				if (!scenario.useVRStep) {


//		if (Input.GetButton ("0"))
//			print ("0");
//		if (Input.GetButton ("1"))
//			print ("1");
//		if (Input.GetButton ("2"))
//			print ("2");
//		if (Input.GetButton ("3"))
//			print ("3");
//		if (Input.GetButton ("4"))
//			print ("4");
//		if (Input.GetButton ("5"))
//			print ("5");
//		if (Input.GetButton ("6"))
//			print ("6");
//		if (Input.GetButton ("7"))
//			print ("7");
//		if (Input.GetButton ("8"))
//			print ("8");
//		if (Input.GetButton ("9"))
//			print ("9");
		
					// ROTATION

					float lh = Input.GetAxis ("JoystickR X");
//					float lh = Input.GetAxis ("Mouse X");


					if (scenario.lockNeck || Input.GetButton ("Follow View")) {
						float headYaw = Camera.main.transform.rotation.eulerAngles.y;
						float neckYaw = neck.transform.rotation.eulerAngles.y;

						transform.RotateTo (0, headYaw, 0);
						neck.relaxedYaw = headYaw - neckYaw;
						neck.transform.RotateTo (0, neckYaw, 0);

					} else if (Mathf.Abs (lh) > .05f) {
						transform.Rotate (0, lh * rotSpeed * Time.deltaTime, 0);
					}

					// HORIZONTAL MOVEMENT

					float h = Input.GetAxis ("Horizontal");
					float v = Input.GetAxis ("Vertical");

					moveDirection = new Vector3 ();

					float crntRunMultiplier = 1;
					if (Input.GetButton ("Run")) {
						crntRunMultiplier = runMultiplier;
					}

					Vector3 forward = transform.TransformDirection (Vector3.forward);
					moveDirection += forward * (((v > 0) ? scenario.fwdSpeed : scenario.fwdSpeed * .5f) * v) * crntRunMultiplier;

					Vector3 right = transform.TransformDirection (Vector3.right);
					moveDirection += right * (scenario.fwdSpeed * h * .7f) * crntRunMultiplier;



					moveDirection *= Time.deltaTime;

					// VERTICAL MOVEMENT

					crntYSpeed -= gravity * Time.deltaTime;

					if (
						characterController.isGrounded &&
						!isStartingJump) {
						crntYSpeed = -gravity * Time.deltaTime * 5;

						if (Input.GetButtonDown ("Jump")) {
							crntYSpeed = 0;
							isStartingJump = true;
						}
					}

					if (isStartingJump) {
			
						if (!Input.GetButton ("Jump"))
							isStartingJump = false;
						else {
				
							crntYSpeed += scenario.jumpAcceleration * Time.deltaTime;

							if (crntYSpeed >= scenario.maxJumpSpeed)
								isStartingJump = false;
						}
					}
					crntYSpeed = Mathf.Clamp (crntYSpeed, -gravity * 30, scenario.maxJumpSpeed);

					moveDirection += new Vector3 (0, crntYSpeed, 0);



					characterController.Move (moveDirection);

				} else {

					float headYaw = Camera.main.transform.rotation.eulerAngles.y;
					float neckYaw = neck.transform.rotation.eulerAngles.y;

					transform.RotateTo (0, headYaw, 0);
					neck.relaxedYaw = headYaw - neckYaw;
					neck.transform.RotateTo (0, neckYaw, 0);

				}
			}

			if (Input.GetButton ("Fire1") && scenario.canShoot) {
			
				Ray ray = Camera.main.ScreenPointToRay (screenCenter);
				RaycastHit hit;

				if (Physics.Raycast (ray, out hit, 1000)) {
					if (hit.collider.gameObject.tag == "Target") {
						Target target = hit.collider.gameObject.GetComponent<Target> ();
						target.shoot (hit);
					}
				}
			}
		}

	}

	void OnTriggerEnter (Collider col)
	{
		
		if(col.gameObject.tag == "Reset Collider")
		{
			if (scenario.isControlled)
				reset ();
		}
		else if(col.gameObject.tag == "Waypoint")
		{
			if (crntWaypointSet != null)
				crntWaypointSet.next ();
		}
	}
}
