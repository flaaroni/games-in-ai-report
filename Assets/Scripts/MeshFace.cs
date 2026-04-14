using ProceduralToolkit;
using System.Collections.Generic;
using UnityEngine;

public class MeshFace : IFace
{
	public MeshFace(GameObject prefab, Transform parent, params Vector3[] verticesClockwise)
	{
		if (verticesClockwise.Length < 3)
		{
			throw new System.ArgumentException($"Expected at least 3 vertices to define a face, but received {verticesClockwise.Length}");
		}

		// Setup member variables
		this.prefab = prefab;
		this.parent = parent;
		this.verticesClockwise = verticesClockwise;

		// Generate edges from a factory and add them to the dictionary
		neighbors = new HashSet<MeshFace>(4);
	}

	public Material Material
	{
		get => setMaterial;
		set
		{
			setMaterial = value;
			if (renderer != null)
			{
				renderer.sharedMaterial = value;
			}
		}
	}

	public void AddNeighbor(MeshFace face)
	{
		// Enforce two-way neighbor relationship
		neighbors.Add(face);
		face.neighbors.Add(this);
	}

	public IEnumerable<IFace> GetNeighbors()
	{
		foreach (MeshFace toReturn in neighbors)
		{
			yield return toReturn;
		}
	}

	public void GenerateMesh()
	{
		if ((renderer != null) || (Material == null))
		{
			return;
		}

		// Create a mesh for this grid cell
		GameObject modelClone = Object.Instantiate(prefab, parent);

		// Reset the model's transform
		modelClone.transform.localPosition = Vector3.zero;
		modelClone.transform.localRotation = Quaternion.identity;
		modelClone.transform.localScale = Vector3.one;

		// Generate a new mesh
		MeshDraft meshDraft = new MeshDraft();
		meshDraft.AddTriangleFan(verticesClockwise, Vector3.back);
		Mesh mesh = meshDraft.ToMesh();
		mesh.name = "QuadFace";

		// Apply to the mesh filter
		modelClone.GetComponent<MeshFilter>().mesh = mesh;

		// Update the material of the mesh
		renderer = modelClone.GetComponent<MeshRenderer>();
		renderer.sharedMaterial = Material;
	}

	// Member variables
	MeshRenderer renderer;
	GameObject prefab;
	Transform parent;
	Material setMaterial;
	Vector3[] verticesClockwise;
	readonly ISet<MeshFace> neighbors;
}
