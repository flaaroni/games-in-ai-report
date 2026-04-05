using UnityEngine;

[CreateAssetMenu(fileName = "ModelGrid.asset", menuName = "AI in Games/Model Grid", order = 1)]
public class ModelGrid : ScriptableObject
{
	[System.Serializable]
	public struct Dimension
	{
		[SerializeField]
		[Range(0.1f, 10f)]
		public float[] gaps;
		[SerializeField]
		[Range(0f, 360f)]
		public float angle;
	}

	[SerializeField]
	Dimension[] dimensions = new Dimension[2];

	public Dimension[] Dimensions => dimensions;
	public Vector3 GetVertex(int dimensionIndex, int vertexIndex)
	{
		Dimension dimension = dimensions[dimensionIndex];
		float totalDistance = 0f;
		for (int i = 0; i < vertexIndex; ++i)
		{
			totalDistance += dimension.gaps[i % dimension.gaps.Length];
		}
		return new Vector3(
			totalDistance * Mathf.Cos(dimension.angle * Mathf.Deg2Rad)
			, totalDistance * Mathf.Sin(dimension.angle * Mathf.Deg2Rad)
			, 0f
		);
	}
}
