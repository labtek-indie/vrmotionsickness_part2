using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Target : MonoBehaviour {

	[SerializeField] Vector3 range = new Vector3(1f, 3f, 1f);

	bool isInitialized = false;
	Vector3 origPosition;

	MeshRenderer meshRenderer;
	Material origMaterial;
	[SerializeField] Material offMaterial;

	public bool IsShot{ get; private set; }
	Rigidbody body;

	void init(){
		if (!isInitialized) {
			origPosition = transform.position;

			meshRenderer = GetComponent<MeshRenderer> ();
			body = GetComponent<Rigidbody> ();

			body.isKinematic = true;

			origMaterial = meshRenderer.material;

			isInitialized = true;
		}
	}

	// Use this for initialization
	void OnEnable () {
		init ();

		IsShot = false;

		transform.position = origPosition;
		transform.rotation = Quaternion.identity;

		body.isKinematic = true;
		meshRenderer.material = origMaterial;

		CancelInvoke();

		shake ();
	}

	void OnDisable(){
		transform.DOKill ();
	}

	void shake(){
		transform.DOMove (
			origPosition + new Vector3 (Random.Range (-range.x, range.x), Random.Range (-range.y, range.y), Random.Range (-range.z, range.z)),
			1f
			)
			.OnComplete (shake);
	}

	public void shoot(RaycastHit hit){
		transform.DOKill ();
		meshRenderer.material = offMaterial;
		body.isKinematic = false;

		body.AddForceAtPosition(hit.normal * -100, hit.point);

		IsShot = true;

		Invoke ("disable", 8);
	}

	void disable(){
		gameObject.SetActive (false);
	}
}
