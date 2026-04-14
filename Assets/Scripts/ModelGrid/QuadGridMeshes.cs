using ProceduralToolkit;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuadGrid.asset", menuName = "AI in Games/Quad Grid", order = 2)]
public class QuadGridMeshes : IGridMeshes
{
	[Header("Settings")]
	[SerializeField]
	[Range(1, 10)]
	int xUnits = 3;
	[SerializeField]
	[Range(1, 10)]
	int yUnits = 3;
	[SerializeField]
	GridDimensions dimensions;

	public int XUnits
	{
		get => xUnits;
		set => xUnits = Mathf.Max(value, 1);
	}
	public int YUnits
	{
		get => yUnits;
		set => yUnits = Mathf.Max(value, 1);
	}
	public GridDimensions Dimensions => dimensions;

	public override void Setup(params int[] numUnitsPerAxis)
	{
		if (numUnitsPerAxis.Length != 2)
		{
			throw new ArgumentException($"Expected 2 parameters for number of units per axis, but received {numUnitsPerAxis.Length}");
		}

		XUnits = numUnitsPerAxis[0];
		YUnits = numUnitsPerAxis[0];
	}

	public override bool IsValid()
	{
		if (dimensions == null)
		{
			return false;
		}
		return true;
	}

	public override ICollection<IFace> Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab)
	{
		// Setup return variables
		List<IFace> toReturn = new List<IFace>(XUnits * YUnits);

		// Destroy all existing children on the parent transform
		foreach (Transform child in parent)
		{
			Destroy(child.gameObject);
		}

		// Go through each grid element
		for (int y = 0; y < YUnits; ++y)
		{
			for (int x = 0; x < XUnits; ++x)
			{
				// Create a group for this grid element
				GameObject groupClone = Instantiate(groupPrefab, parent);

				// Position the group at the correct location based on the grid dimensions
				groupClone.transform.localPosition = GetVertex(x, y);
				groupClone.transform.localRotation = Quaternion.identity;
				groupClone.transform.localScale = Vector3.one;

				// Name the group
				groupClone.name = $"Grid Element ({x}, {y})";

				// Create a mesh for this grid element
				QuadFace newFace = GenerateGridCell(groupClone.transform, modelPrefab, x, y);

				// Add neighbors to this face
				if (x > 0)
				{
					newFace.AddNeighbor((QuadFace)toReturn[toReturn.Count - 1]);
				}
				if (y > 0)
				{
					newFace.AddNeighbor((QuadFace)toReturn[toReturn.Count - XUnits]);
				}

				// Add face to the return list
				toReturn.Add(newFace);
			}
		}
		return toReturn;
	}

	/// <summary>
	/// Calculates the position of a vertex in a quadrilateral grid based on the specified x and y indices.
	/// </summary>
	/// <param name="x">The horizontal index of the vertex within the grid</param>
	/// <param name="y">The vertical index of the vertex within the grid</param>
	/// <returns>A Vector2 representing the position of the specified vertex in the grid.</returns>
	Vector2 GetVertex(int x, int y)
	{
		return Dimensions.X.GetVector(x) + Dimensions.Y.GetVector(y);
	}

	QuadFace GenerateGridCell(Transform parent, GameObject modelPrefab, int x, int y)
	{
		// Create a mesh for this grid cell
		GameObject modelClone = Instantiate(modelPrefab, parent);

		// Reset the model's transform
		modelClone.transform.localPosition = Vector3.zero;
		modelClone.transform.localRotation = Quaternion.identity;
		modelClone.transform.localScale = Vector3.one;

		// Name the model
		modelClone.name = $"SubMesh ({x}, {y})";

		// Retrieve the mesh filter
		MeshFilter meshFilter = modelClone.GetComponent<MeshFilter>();
		Vector2 xEnd = Dimensions.X.GetVector(),
			yEnd = Dimensions.Y.GetVector();
		meshFilter.mesh = MeshDraft.Quad(Vector2.zero, yEnd, (xEnd + yEnd), xEnd).ToMesh();

		// Create a QuadFace for this grid cell and store it in the return array
		return new QuadFace(modelClone, x, y);
	}
}
