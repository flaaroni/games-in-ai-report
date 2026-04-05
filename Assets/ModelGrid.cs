using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ModelGrid.asset", menuName = "AI in Games/Model Grid", order = 1)]
public class ModelGrid : ScriptableObject
{
	[System.Serializable]
	public struct Dimension
	{
		// The first gap
		[SerializeField]
		[Range(0.1f, 10f)]
		public float gap;
		[SerializeField]
		[Range(0f, 360f)]
		public float angle;
	}

	[SerializeField]
	Dimension[] dimensions = new Dimension[2];

	public Dimension[] Dimensions => dimensions;
	public int NumDimensions => dimensions.Length;
	public Vector3 GetDimensionVector(int dimensionIndex)
	{
		Dimension dimension = dimensions[dimensionIndex];
		return new Vector3(
			dimension.gap * Mathf.Cos(dimension.angle * Mathf.Deg2Rad)
			, dimension.gap * Mathf.Sin(dimension.angle * Mathf.Deg2Rad)
			, 0f
		);
	}

	public Vector3 GetVertex(params int[] vertexIndices)
	{
		if (vertexIndices.Length != dimensions.Length)
		{
			throw new System.ArgumentException($"Invalid vertex indices length {vertexIndices.Length} for dimensions length {dimensions.Length}");

		}

		Vector3 toReturn = Vector3.zero;
		for (int i = 0; i < vertexIndices.Length; ++i)
		{
			toReturn += vertexIndices[i] * GetDimensionVector(i);
		}
		return toReturn;
	}
}
