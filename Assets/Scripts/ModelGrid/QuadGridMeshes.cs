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
				groupClone.transform.localPosition = Dimensions.X.GetVector(x) + Dimensions.Y.GetVector(y);
				groupClone.transform.localRotation = Quaternion.identity;
				groupClone.transform.localScale = Vector3.one;

				// Name the group
				groupClone.name = $"Grid Element ({x}, {y})";

				// Create a mesh for this grid element
				Vector2 xEnd = Dimensions.X.GetVector(x + 1) - Dimensions.X.GetVector(x),
					yEnd = Dimensions.Y.GetVector(y + 1) - Dimensions.Y.GetVector(y);
				MeshFace newFace = new MeshFace(modelPrefab, groupClone.transform, Vector2.zero, yEnd, (xEnd + yEnd), xEnd);

				// Add neighbors to this face
				if (x > 0)
				{
					newFace.AddNeighbor((MeshFace)toReturn[toReturn.Count - 1]);
				}
				if (y > 0)
				{
					newFace.AddNeighbor((MeshFace)toReturn[toReturn.Count - XUnits]);
				}

				// Add face to the return list
				toReturn.Add(newFace);
			}
		}
		return toReturn;
	}
}
