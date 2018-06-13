using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum eOrientationMode { NODE = 0, TANGENT, NONE }

[AddComponentMenu("Splines/Spline Controller")]
[RequireComponent(typeof(SplineInterpolator))]
public class SplineController : MonoBehaviour
{
	public GameObject SplineRoot;
	public float Duration = 10;
	public eOrientationMode OrientationMode = eOrientationMode.NODE;
	public eWrapMode WrapMode = eWrapMode.ONCE;
	public float rotationLookAhead = 1f;
	public bool AutoStart = true;
	public bool AutoClose = true;
	public bool HideOnExecute = true;


	SplineInterpolator mSplineInterp;
	[SerializeField] SplineInterpolator mSplineDrawer;
	Transform[] mTransforms;

	void OnDrawGizmos()
	{
		if (mSplineDrawer != null) {
			Transform[] trans = GetTransforms ();
			if (trans == null || trans.Length < 2)
				return;
			
			SetupSplineInterpolator (mSplineDrawer, trans);
			mSplineDrawer.StartInterpolation (null, WrapMode, false, true);


			Vector3 prevPos = trans [0].position;
			for (int c = 1; c <= 100; c++) {
				float currTime = c * Duration / 100;
				Vector3 currPos = mSplineDrawer.GetHermiteAtTime (currTime);
				float mag = (currPos - prevPos).magnitude * 2;
				Gizmos.color = new Color (mag, 0, 0, 1);
				Gizmos.DrawLine (prevPos, currPos);
				prevPos = currPos;
			}
		}
	}

	void Start()
	{
		if (AutoStart)
			init();
	}

	public void stop(){
		if (mSplineInterp != null)
			mSplineInterp.Reset ();
	}

	public void init(GameObject splineRoot = null){
		if (splineRoot != null)
			SplineRoot = splineRoot;

		SplineRoot.SetActive (true);

		mSplineInterp = GetComponent<SplineInterpolator> ();

		mTransforms = GetTransforms();

		if (HideOnExecute)
			DisableTransforms();
		
		FollowSpline();
	}

	/// <summary>
	/// Starts the interpolation
	/// </summary>
	void FollowSpline()
	{
		if (mTransforms.Length > 0)
		{
			SetupSplineInterpolator(mSplineInterp, mTransforms);
			mSplineInterp.StartInterpolation(null, WrapMode, rotates: OrientationMode != eOrientationMode.NONE, rotationLookAhead: rotationLookAhead);
		}
	}
	/// <summary>
	/// Process transforms to points and add them to the interpolator
	/// </summary>
	/// <param name="interp">Interp.</param>
	/// <param name="trans">Trans.</param>
	void SetupSplineInterpolator(SplineInterpolator interp, Transform[] trans)
	{
		interp.Reset();

		float step = (AutoClose) ? Duration / trans.Length :
			Duration / (trans.Length - 1);

		int c;
		for (c = 0; c < trans.Length; c++)
		{
			if (OrientationMode == eOrientationMode.NONE)
			{
				interp.AddPoint(trans[c].position, trans[c].rotation, step * c, new Vector2(0, 1));
			}
			else if (OrientationMode == eOrientationMode.NODE)
			{
				interp.AddPoint(trans[c].position, Quaternion.identity, step * c, new Vector2(0, 1));
			}
			else if (OrientationMode == eOrientationMode.TANGENT)
			{
				Quaternion rot;
				if (c != trans.Length - 1)
					rot = Quaternion.LookRotation(trans[c + 1].position - trans[c].position, trans[c].up);
				else if (AutoClose)
					rot = Quaternion.LookRotation(trans[0].position - trans[c].position, trans[c].up);
				else
					rot = trans[c].rotation;

				rot = Quaternion.Euler (new Vector3 (0, rot.eulerAngles.y, 0));

				interp.AddPoint(trans[c].position, rot, step * c, new Vector2(0, 1));
			}
		}

		if (AutoClose)
			interp.SetAutoCloseMode(step * c);
	}


	/// <summary>
	/// Returns children transforms, sorted by name.
	/// </summary>
	Transform[] GetTransforms()
	{
		if (SplineRoot != null)
		{
			SplineNode[] splineNodes = SplineRoot.GetComponentsInChildren<SplineNode>();
			Transform[] transforms = new Transform[splineNodes.Length];

			for (int i = 0; i < splineNodes.Length; i++) {
				transforms [i] = splineNodes [i].transform;
			}

			return transforms;
		}

		return null;
	}

	/// <summary>
	/// Disables the spline objects, we don't need them outside design-time.
	/// </summary>
	void DisableTransforms()
	{
		if (SplineRoot != null)
		{
			SplineRoot.SetActive(false);
		}
	}


}