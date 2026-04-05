using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using static ModelGrid;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCreator : MonoBehaviour
{
	[SerializeField]
	ModelGrid grid;
	[SerializeField]
	WaveFunctionCollapse algorithm;

	[Header("Debug")]
	[SerializeField]
	GameObject testVertex;

	Mesh mesh;
	MeshFilter meshFilter;

	public Mesh Mesh => mesh ??= new Mesh();
	public MeshFilter Filter
	{
		get
		{
			if (meshFilter == null)
			{
				meshFilter = GetComponent<MeshFilter>();
				meshFilter.mesh = Mesh;
			}
			return meshFilter;
		}
	}

	public int NumVertices(int dimension)
	{
		// FIXME: Support more than 2 dimensions
		switch (dimension)
		{
			case 0:
				return algorithm.SegmentsX.GetLength(0);
			case 1:
			default:
				return algorithm.SegmentsY.GetLength(0);
		}
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	IEnumerator Start()
	{
		yield return null;
		// FIXME: support more than 2 dimensions
		//for (int dimension = 0; dimension < grid.NumDimensions; ++dimension)
		//{
		//}
		for (int x = 0; x < NumVertices(0); ++x)
		{
			for (int y = 0; y < NumVertices(1); ++y)
			{
				GameObject clone = Instantiate(testVertex, transform);
				clone.transform.localPosition = grid.GetVertex(x, y);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
