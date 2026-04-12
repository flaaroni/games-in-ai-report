using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class storing data for a single face of a mesh.
/// </summary>
public abstract class MeshFace : IFace
{
	public MeshFace(GameObject model)
	{
		Model = model;
	}

	public GameObject Model { get; }
	public Material Material
	{
		get => Renderer.sharedMaterial;
		set => Renderer.sharedMaterial = value;
	}

	/// <summary>
	/// Represents the edges of the face as a dictionary.
	/// Keys indicates the edge index, and the value represents
	/// the left or right side of the edge.
	/// </summary>
	public abstract IDictionary<IEdge, IEdge.Side> Edges { get; }
	/// <summary>
	/// Represents the neighbors as a dictionary.
	/// Keys indicates the edge the neighbors share,
	/// and the value represents the neighbor itself.
	/// </summary>
	public abstract IDictionary<IEdge, IFace> Neighbors { get; }

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

	public bool Equals(IFace other)
	{
		if (other is MeshFace)
		{
			return Model == ((MeshFace)other).Model;
		}
		return false;
	}
}
