using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "GridDimensions.asset", menuName = "AI in Games/Grid Dimensions", order = 1)]
public class GridDimensions : ScriptableObject
{
	[System.Serializable]
	public class XDimension : IDimension
	{
		[SerializeField]
		[Range(0.1f, 10f)]
		[FormerlySerializedAs("gap")]
		float length = 1;

		[SerializeField]
		[Range(0.1f, 0.9f)]
		float[] subdivisions = new float[] { };

		public float Length => length;
		public ICollection<float> Subdivisions => subdivisions;
		public virtual float Angle => 0f;
		public virtual Vector2 GetVector() => new Vector2(Length, 0f);
	}

	[System.Serializable]
	public class YDimension : XDimension
	{
		[SerializeField]
		[Range(0f, 180f)]
		float angle = 90f;

		public override float Angle => angle;
		public override Vector2 GetVector() => new Vector2(
			Length * Mathf.Cos(Angle * Mathf.Deg2Rad)
			, Length * Mathf.Sin(Angle * Mathf.Deg2Rad)
		);
	}

	[System.Serializable]
	public class ZDimension : IDimension
	{
		[SerializeField]
		[Range(0.1f, 0.9f)]
		float[] subdivisions = new float[] { };

		XDimension xDimension = null;
		YDimension yDimension = null;

		public float Length => GetVector().magnitude;
		public virtual float Angle => Vector2.Angle(Vector2.right, GetVector());
		public ICollection<float> Subdivisions => subdivisions;

		public Vector2 GetVector() => xDimension.GetVector() - yDimension.GetVector();
		public void Initialize(XDimension xDimension, YDimension yDimension)
		{
			this.xDimension = xDimension;
			this.yDimension = yDimension;
		}
		public bool IsInitialized => xDimension != null && yDimension != null;
	}

	[SerializeField]
	XDimension xDimension;
	[SerializeField]
	YDimension yDimension;
	[SerializeField]
	ZDimension zDimension;

	public IDimension X => xDimension;
	public IDimension Y => yDimension;
	public IDimension Z
	{
		get
		{
			if (!zDimension.IsInitialized)
			{
				zDimension.Initialize(xDimension, yDimension);
			}
			return zDimension;
		}
	}
}
