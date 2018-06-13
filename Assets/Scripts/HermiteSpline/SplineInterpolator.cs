using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum eEndPointsMode { AUTO, AUTOCLOSED, EXPLICIT }
public enum eWrapMode { ONCE, LOOP }
public delegate void OnEndCallback();

public class SplineInterpolator : MonoBehaviour
{
	bool _isDrawer = false;
	eEndPointsMode mEndPointsMode = eEndPointsMode.AUTO;

	bool _rotates;
	float _rotationLookAhead = 0;


	void OnEnable(){
		AppMaster.OnAppStep += step;
	}

	void OnDisable(){
		AppMaster.OnAppStep -= step;
	}


	internal class SplineNode
	{
		internal Vector3 Point;
		internal Quaternion Rot;
		internal float Time;
		internal Vector2 EaseIO;

		internal SplineNode(Vector3 p, Quaternion q, float t, Vector2 io) { Point = p; Rot = q; Time = t; EaseIO = io; }
		internal SplineNode(SplineNode o) { Point = o.Point; Rot = o.Rot; Time = o.Time; EaseIO = o.EaseIO; }
	}

	List<SplineNode> mNodes = new List<SplineNode>();
	string mState = "";

	OnEndCallback mOnEndCallback;



	public void StartInterpolation(OnEndCallback endCallback, eWrapMode mode, bool rotates = false, bool isDrawer = false, float rotationLookAhead = 0)
	{
		_rotates = rotates;
		_rotationLookAhead = rotationLookAhead;
		_isDrawer = isDrawer;

		if (mState != "Reset")
			throw new System.Exception("First reset, add points and then call here");

		mState = mode == eWrapMode.ONCE ? "Once" : "Loop";
		mOnEndCallback = endCallback;

		SetInput();
	}

	public void Reset()
	{
		mNodes.Clear();
		mState = "Reset";
		mCurrentIdx = 1;
		mCurrentTime = 0;

		_rotates = false;
		_rotationLookAhead = 0;

		mEndPointsMode = eEndPointsMode.AUTO;
	}

	public void AddPoint(Vector3 pos, Quaternion quat, float timeInSeconds, Vector2 easeInOut)
	{
		if (mState != "Reset")
			throw new System.Exception("Cannot add points after start");

		mNodes.Add(new SplineNode(pos, quat, timeInSeconds, easeInOut));
	}


	void SetInput()
	{
		if (mNodes.Count < 2)
			throw new System.Exception("Invalid number of points");

		if (!_isDrawer)
		{
			for (int c = 1; c < mNodes.Count; c++)
			{
				SplineNode node = mNodes[c];
				SplineNode prevNode = mNodes[c - 1];

				// Always interpolate using the shortest path -> Selective negation
				if (Quaternion.Dot(node.Rot, prevNode.Rot) < 0)
				{
					node.Rot.x = -node.Rot.x;
					node.Rot.y = -node.Rot.y;
					node.Rot.z = -node.Rot.z;
					node.Rot.w = -node.Rot.w;
				}
			}
		}

		if (mEndPointsMode == eEndPointsMode.AUTO)
		{
			mNodes.Insert(0, mNodes[0]);
			mNodes.Add(mNodes[mNodes.Count - 1]);
		}
		else if (mEndPointsMode == eEndPointsMode.EXPLICIT && (mNodes.Count < 4))
			throw new System.Exception("Invalid number of points");
	}

	void SetExplicitMode()
	{
		if (mState != "Reset")
			throw new System.Exception("Cannot change mode after start");

		mEndPointsMode = eEndPointsMode.EXPLICIT;
	}

	public void SetAutoCloseMode(float joiningPointTime)
	{
		if (mState != "Reset")
			throw new System.Exception("Cannot change mode after start");

		mEndPointsMode = eEndPointsMode.AUTOCLOSED;

		mNodes.Add(new SplineNode(mNodes[0] as SplineNode));
		mNodes[mNodes.Count - 1].Time = joiningPointTime;

		Vector3 vInitDir = (mNodes[1].Point - mNodes[0].Point).normalized;
		Vector3 vEndDir = (mNodes[mNodes.Count - 2].Point - mNodes[mNodes.Count - 1].Point).normalized;
		float firstLength = (mNodes[1].Point - mNodes[0].Point).magnitude;
		float lastLength = (mNodes[mNodes.Count - 2].Point - mNodes[mNodes.Count - 1].Point).magnitude;

		SplineNode firstNode = new SplineNode(mNodes[0] as SplineNode);
		firstNode.Point = mNodes[0].Point + vEndDir * firstLength;

		SplineNode lastNode = new SplineNode(mNodes[mNodes.Count - 1] as SplineNode);
		lastNode.Point = mNodes[0].Point + vInitDir * lastLength;

		mNodes.Insert(0, firstNode);
		mNodes.Add(lastNode);
	}

	float mCurrentTime;
	int mCurrentIdx = 1;

	void step()
	{
		if (!_isDrawer) {
			if (mState == "Reset" || mState == "Stopped" || mNodes.Count < 4)
				return;

			int nextIdx = mCurrentIdx;
			float nextTime = mCurrentTime;
			string nextState = mState;

			nextTime += Time.deltaTime;

			// We advance to next point in the path
			if (nextTime >= mNodes [nextIdx + 1].Time) {
				if (nextIdx < mNodes.Count - 3) {
					nextIdx++;
				} else {
					if (nextState != "Loop") {
						nextState = "Stopped";

						// We stop right in the end point
						transform.position = mNodes [mNodes.Count - 2].Point;

						if(_rotates)
							transform.rotation = mNodes [mNodes.Count - 2].Rot;

						// We call back to inform that we are ended
						if (mOnEndCallback != null)
							mOnEndCallback ();
					} else {
						nextIdx = 1;
						nextTime = 0;
					}
				}
			}

			if (nextState != "Stopped") {
				// Calculates the t param between 0 and 1
				float param = (nextTime - mNodes [nextIdx].Time) / (mNodes [nextIdx + 1].Time - mNodes [nextIdx].Time);

				// Smooth the param
				param = MathUtils.Ease (param, mNodes [nextIdx].EaseIO.x, mNodes [nextIdx].EaseIO.y);

				transform.position = GetHermiteInternal (nextIdx, param);

				if(_rotates)
					transform.rotation = GetSquad (nextIdx, param);
			}

			//

			mCurrentIdx = nextIdx;
			mCurrentTime = nextTime;
			mState = nextState;


			nextTime += _rotationLookAhead;

			if (_rotates && _rotationLookAhead > 0) {
				// We advance to next point in the path
				while (nextTime >= mNodes [nextIdx + 1].Time) {
					if (nextIdx < mNodes.Count - 3) {
						nextIdx++;
					} else {
						if (nextState != "Loop") {
							nextState = "Stopped";

							transform.rotation = mNodes [mNodes.Count - 2].Rot;

							// We call back to inform that we are ended
							if (mOnEndCallback != null)
								mOnEndCallback ();
						} else {
							nextIdx = 1;
							nextTime = 0;
						}

						break;
					}
				}

				if (nextState != "Stopped") {
					// Calculates the t param between 0 and 1
					float param = (nextTime - mNodes [nextIdx].Time) / (mNodes [nextIdx + 1].Time - mNodes [nextIdx].Time);

					// Smooth the param
					param = MathUtils.Ease (param, mNodes [nextIdx].EaseIO.x, mNodes [nextIdx].EaseIO.y);

					transform.rotation = GetSquad (nextIdx, param);
				}
			}
		}
	}

	Quaternion GetSquad(int idxFirstPoint, float t)
	{
		Quaternion Q0 = mNodes[idxFirstPoint - 1].Rot;
		Quaternion Q1 = mNodes[idxFirstPoint].Rot;
		Quaternion Q2 = mNodes[idxFirstPoint + 1].Rot;
		Quaternion Q3 = mNodes[idxFirstPoint + 2].Rot;

		Quaternion T1 = MathUtils.GetSquadIntermediate(Q0, Q1, Q2);
		Quaternion T2 = MathUtils.GetSquadIntermediate(Q1, Q2, Q3);

		return MathUtils.GetQuatSquad(t, Q1, Q2, T1, T2);
	}



	public Vector3 GetHermiteInternal(int idxFirstPoint, float t)
	{
		float t2 = t * t;
		float t3 = t2 * t;

		Vector3 P0 = mNodes[idxFirstPoint - 1].Point;
		Vector3 P1 = mNodes[idxFirstPoint].Point;
		Vector3 P2 = mNodes[idxFirstPoint + 1].Point;
		Vector3 P3 = mNodes[idxFirstPoint + 2].Point;

		float tension = 0.5f;	// 0.5 equivale a catmull-rom

		Vector3 T1 = tension * (P2 - P0);
		Vector3 T2 = tension * (P3 - P1);

		float Blend1 = 2 * t3 - 3 * t2 + 1;
		float Blend2 = -2 * t3 + 3 * t2;
		float Blend3 = t3 - 2 * t2 + t;
		float Blend4 = t3 - t2;

		return Blend1 * P1 + Blend2 * P2 + Blend3 * T1 + Blend4 * T2;
	}


	public Vector3 GetHermiteAtTime(float timeParam)
	{
		if (timeParam >= mNodes[mNodes.Count - 2].Time)
			return mNodes[mNodes.Count - 2].Point;

		int c;
		for (c = 1; c < mNodes.Count - 2; c++)
		{
			if (mNodes[c].Time > timeParam)
				break;
		}

		int idx = c - 1;
		float param = (timeParam - mNodes[idx].Time) / (mNodes[idx + 1].Time - mNodes[idx].Time);
		param = MathUtils.Ease(param, mNodes[idx].EaseIO.x, mNodes[idx].EaseIO.y);

		return GetHermiteInternal(idx, param);
	}
}