using System.Collections.Generic;
using UnityEngine;

public class QuadFace : IFace
{
	public QuadFace(int x, int y, GameObject model)
	{
		// Setup member variables
		X = x;
		Y = y;
		this.model = model;

		// Setup data structures for edges and neighbors
		neighbors = new(4);

		// FIXME: do NOT generate a new edge,
		// but rather, grab it from a collection or factory
		edges = new(4)
		{
			{ new QuadEdge {
				axis = QuadEdge.Axis.X,
				X = x,
				Y = y,
			}, IEdge.Side.Left },
			{ new QuadEdge {
				axis = QuadEdge.Axis.Y,
				X = x,
				Y = y,
			}, IEdge.Side.Right },
			{ new QuadEdge {
				axis = QuadEdge.Axis.X,
				X = x,
				Y = y + 1,
			}, IEdge.Side.Right },
			{ new QuadEdge {
				axis = QuadEdge.Axis.Y,
				X = x + 1,
				Y = y,
			}, IEdge.Side.Left },
		};
	}

	public IReadOnlyDictionary<IEdge, IEdge.Side> Edges => edges;

	public IReadOnlyDictionary<IEdge, IFace> Neighbors => neighbors;

	public Material Material
	{
		get => Renderer.sharedMaterial;
		set => Renderer.sharedMaterial = value;
	}

	public int X { get; }
	public int Y { get; }
	public bool AddNeighbor(QuadEdge edge, QuadFace neighbor) => neighbors.TryAdd(edge, neighbor);

	public override int GetHashCode() => model.GetHashCode();

	public override bool Equals(object other)
	{
		if (other is QuadFace)
		{
			return model == ((QuadFace)other).model;
		}
		return false;
	}

	public bool Equals(IFace other)
	{
		return Equals((object)other);
	}

	protected MeshRenderer Renderer
	{
		get
		{
			// Cache the renderer reference for later use
			if (renderer == null)
			{
				renderer = model.GetComponent<MeshRenderer>();
			}
			return renderer;
		}
	}

	// Member variables
	MeshRenderer renderer;
	readonly GameObject model;
	readonly Dictionary<IEdge, IEdge.Side> edges;
	readonly Dictionary<IEdge, IFace> neighbors;
}
