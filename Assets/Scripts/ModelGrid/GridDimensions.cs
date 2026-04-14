using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GridDimensions.asset", menuName = "AI in Games/Grid Dimensions", order = 1)]
public class GridDimensions : ScriptableObject
{
	[Serializable]
	public class XDimension : IDimension
	{
		[SerializeField]
		[Range(0.1f, 10f)]
		[FormerlySerializedAs("length")]
		float firstLength = 1;

		[SerializeField]
		[Range(0.1f, 0.9f)]
		float[] loopingLengths = new float[] { };

		protected float GetLength(int index)
		{
			if (index < 0)
			{
				return 0f;
			}

			int numLengths = loopingLengths.Length + 1;
			int multiplier = index / numLengths;
			int addIndex = index % numLengths;

			float toReturn = firstLength * (multiplier + (addIndex >= 1 ? 1 : 0));
			for (int i = 0; i < loopingLengths.Length; ++i)
			{
				toReturn += loopingLengths[i] * (multiplier + (addIndex >= (i + 2) ? 1 : 0));
			}
			return toReturn;
		}

		public virtual Vector2 GetVector(int index)
		{
			return new Vector2(GetLength(index), 0f);
		}

		protected int GetLocalIndex(int index) => index % (loopingLengths.Length + 1);
	}

	[Serializable]
	public class YDimension : XDimension
	{
		[SerializeField]
		[Range(0f, 180f)]
		float angle = 90f;

		protected float Angle => angle;
		public override Vector2 GetVector(int index)
		{
			return new Vector2(
				GetLength(index) * Mathf.Cos(Angle * Mathf.Deg2Rad)
				, GetLength(index) * Mathf.Sin(Angle * Mathf.Deg2Rad)
			);
		}
	}

	//[Serializable]
	//public class ZDimension : IDimension
	//{
	//	[SerializeField]
	//	[Range(0.1f, 10f)]
	//	float firstLength = 1;

	//	[SerializeField]
	//	[Range(0.1f, 0.9f)]
	//	float[] loopingLengths = new float[] { };

	//	XDimension xDimension = null;
	//	YDimension yDimension = null;

	//	public float Length => GetVector().magnitude;
	//	public virtual float Angle => Vector2.Angle(Vector2.right, GetVector());

	//	public Vector2 GetVector() => xDimension.GetVector() - yDimension.GetVector();
	//	public void Initialize(XDimension xDimension, YDimension yDimension)
	//	{
	//		this.xDimension = xDimension;
	//		this.yDimension = yDimension;
	//	}
	//	public bool IsInitialized => xDimension != null && yDimension != null;
	//}

	[SerializeField]
	XDimension xDimension;
	[SerializeField]
	YDimension yDimension;
	//[SerializeField]
	//ZDimension zDimension;

	public IDimension X => xDimension;
	public IDimension Y => yDimension;
	//public IDimension Z
	//{
	//	get
	//	{
	//		if (!zDimension.IsInitialized)
	//		{
	//			zDimension.Initialize(xDimension, yDimension);
	//		}
	//		return zDimension;
	//	}
	//}
}
