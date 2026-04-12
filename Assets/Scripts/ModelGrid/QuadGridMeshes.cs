using System;
using UnityEngine;
using ProceduralToolkit;

[CreateAssetMenu(fileName = "QuadGrid.asset", menuName = "AI in Games/Quad Grid", order = 2)]
public class QuadGridMeshes : ScriptableObject
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

	[Header("Render")]
	[SerializeField]
	Mesh[,] subMeshes;
	[SerializeField]
	Material[] allMaterials;

	public int XUnits
	{
		get => xUnits;
		set => xUnits = value;
	}
	public int YUnits
	{
		get => yUnits;
		set => yUnits = value;
	}
	public Material[] AllMaterials => allMaterials;
	public GridDimensions Dimensions => dimensions;

	public Mesh[,] SubMeshes
	{
		get
		{
			if ((subMeshes == null)
				|| (subMeshes.GetLength(0) != XSubdivisions)
				|| (subMeshes.GetLength(1) != YSubdivisions))
			{
				subMeshes = GenerateSubMeshes();
			}
			return subMeshes;
		}
	}

	public bool IsValid()
	{
		if (dimensions == null)
		{
			return false;
		}
		for (int y = 0; y < SubMeshes.GetLength(1); ++y)
		{
			for (int x = 0; x < SubMeshes.GetLength(0); ++x)
			{
				if (SubMeshes[x, y] == null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public QuadFace[,] GenerateGrid(Transform parent, GameObject groupPrefab, GameObject modelPrefab)
	{
		// Setup return variables
		QuadFace[,] toReturn = new QuadFace[(XUnits * XSubdivisions), (YUnits * YSubdivisions)];

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
				GenerateGridCell(groupClone.transform, modelPrefab, (x * XSubdivisions), (y * YSubdivisions), ref toReturn);
			}
		}
		return toReturn;
	}

	int XSubdivisions => Dimensions.X.NumSubdivisions() + 1;
	int YSubdivisions => Dimensions.Y.NumSubdivisions() + 1;

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

	void GenerateGridCell(Transform parent, GameObject modelPrefab, int xOffset, int yOffset, ref QuadFace[,] toReturn)
	{
		// Go through all subdivisions of the grid cell
		for (int y = 0; y < YSubdivisions; ++y)
		{
			for (int x = 0; x < XSubdivisions; ++x)
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
				meshFilter.mesh = SubMeshes[x, y];

				// Create a QuadFace for this grid cell and store it in the return array
				int faceX = (xOffset + x), faceY = (yOffset + y);
				toReturn[faceX, faceY] = new QuadFace(faceX, faceY, modelClone);

				// Add neighbors to the left and below, if they exist
				if (faceX > 0)
				{
					QuadEdge sharedEdge = new() {
						axis = QuadEdge.Axis.Y,
						X = faceX,
						Y = faceY,
					};
					toReturn[faceX, faceY].AddNeighbor(sharedEdge, toReturn[faceX - 1, faceY]);
					toReturn[faceX - 1, faceY].AddNeighbor(sharedEdge, toReturn[faceX, faceY]);
				}
				if (faceY > 0)
				{
					QuadEdge sharedEdge = new()
					{
						axis = QuadEdge.Axis.X,
						X = faceX,
						Y = faceY,
					};
					toReturn[faceX, faceY - 1].AddNeighbor(sharedEdge, toReturn[faceX, faceY]);
					toReturn[faceX, faceY].AddNeighbor(sharedEdge, toReturn[faceX, faceY - 1]);
				}
			}
		}
	}

	Mesh[,] GenerateSubMeshes()
	{
		// Go through all subdivisions of the grid cell
		Mesh[,] toReturn = new Mesh[XSubdivisions, YSubdivisions];
		for (int y = 0; y < YSubdivisions; ++y)
		{
			for (int x = 0; x < XSubdivisions; ++x)
			{
				// Generate vertices in clockwise direction for quads
				var (xStart, xEnd) = Dimensions.X.GetSubVector(x);
				var (yStart, yEnd) = Dimensions.Y.GetSubVector(y);
				toReturn[x, y] = MeshDraft.Quad((xStart + yStart), (xStart + yEnd), (xEnd + yEnd), (xEnd + yStart)).ToMesh();
			}
		}
		return toReturn;
	}
}
