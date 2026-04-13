using System.Collections.Generic;
using UnityEngine;

public class QuadFace : IFace
{
	public QuadFace(GameObject model, int x, int y, QuadEdgeFactory factory)
	{
		// Setup member variables
		X = x;
		Y = y;
		this.model = model;

		// Generate edges from a factory and add them to the dictionary
		edges = new(4);
		AddEdge(factory.GetEdge(QuadEdge.Axis.X, x, y), IEdge.Side.Left);
		AddEdge(factory.GetEdge(QuadEdge.Axis.Y, x, y), IEdge.Side.Right);
		AddEdge(factory.GetEdge(QuadEdge.Axis.X, x, y + 1), IEdge.Side.Right);
		AddEdge(factory.GetEdge(QuadEdge.Axis.Y, x + 1, y), IEdge.Side.Left);
	}

	public IReadOnlyDictionary<IEdge, IEdge.Side> Edges => edges;

	public IFace GetNeighbor(IEdge edge)
	{
		// Attempt to get the edge
		bool isValidEdge = edges.TryGetValue(edge, out IEdge.Side side);
		if (!isValidEdge)
		{
			return null;
		}

		// Get the opposite side of the edge (i.e. if it's left, get right, and vice versa)
		side = (IEdge.Side)((byte)(side + 1) % IEdge.NUM_SIDES);

		// Return the neighbor on the opposite side
		return edge.Faces.TryGetValue(side, out IFace leftNeighbor) ? leftNeighbor : null;
	}

	public Material Material
	{
		get => Renderer.sharedMaterial;
		set => Renderer.sharedMaterial = value;
	}

	public int X { get; }
	public int Y { get; }

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

	protected void AddEdge(QuadEdge edge, IEdge.Side side)
	{
		edges.Add(edge, side);
		edge.AddFace(side, this);
	}

	protected MeshRenderer Renderer
	{
		get
		{
			// Cache the renderer reference for later use
			if (rendererCache == null)
			{
				rendererCache = model.GetComponent<MeshRenderer>();
			}
			return rendererCache;
		}
	}

	// Member variables
	MeshRenderer rendererCache;
	readonly GameObject model;
	readonly Dictionary<IEdge, IEdge.Side> edges;
}
