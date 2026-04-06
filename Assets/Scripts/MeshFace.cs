using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class storing data for a single face of a mesh.
/// </summary>
public abstract class MeshFace<EDGE, NEIGHBOR>
{
	public enum Side : byte { Left, Right }

	public MeshFace(GameObject model, byte numMaterials)
	{
		Model = model;

		// Setup all MeshFaces to have all possibilities defined
		// at the start, and then remove possibilities as they are ruled out
		possibleMaterials = new SortedSet<byte>();
		for(byte i = 0; i < numMaterials; ++i)
		{
			possibleMaterials.Add(i);
		}
	}

	public GameObject Model { get; }
	public Material Material
	{
		get => Renderer.sharedMaterial;
		set => Renderer.sharedMaterial = value;
	}

	public ISet<byte> PossibleMaterials => possibleMaterials;
	public bool IsCollapsed => possibleMaterials.Count == 1;
	public bool IsEmpty => possibleMaterials.Count == 0;

	/// <summary>
	/// Represents the edges of the face as a dictionary.
	/// Keys indicates the edge index, and the value represents
	/// the left or right side of the edge.
	/// </summary>
	public abstract IDictionary<EDGE, Side> Edges { get; }
	/// <summary>
	/// Represents the neighbors as a dictionary.
	/// Keys indicates the edge the neighbors share,
	/// and the value represents the neighbor itself.
	/// </summary>
	public abstract IDictionary<EDGE, NEIGHBOR> GetNeighbors();

	protected MeshRenderer Renderer
	{
		get
		{
			// Cache the renderer reference for later use
			if (renderer == null)
			{
				renderer = Model.GetComponent<MeshRenderer>();
			}
			return renderer;
		}
	}

	// Member variables
	MeshRenderer renderer;
	SortedSet<byte> possibleMaterials;
}
