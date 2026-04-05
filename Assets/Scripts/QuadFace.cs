using System.Collections.Generic;
using UnityEngine;

public class QuadFace : MeshFace<QuadEdge, QuadFace>
{
	public QuadFace(int x, int y, GameObject model, byte numMaterials) : base(model, numMaterials)
	{
		// Setup member variables
		X = x;
		Y = y;

		// Setup data structures for edges and neighbors
		neighbors = new(4);
		edges = new(4)
		{
			{ new QuadEdge {
				axis = QuadEdge.Axis.X,
				X = x,
				Y = y,
			}, Side.Left },
			{ new QuadEdge {
				axis = QuadEdge.Axis.Y,
				X = x,
				Y = y,
			}, Side.Right },
			{ new QuadEdge {
				axis = QuadEdge.Axis.X,
				X = x,
				Y = y + 1,
			}, Side.Right },
			{ new QuadEdge {
				axis = QuadEdge.Axis.Y,
				X = x + 1,
				Y = y,
			}, Side.Left },
		};
	}

	public override IDictionary<QuadEdge, Side> Edges => edges;

	public override IDictionary<QuadEdge, QuadFace> GetNeighbors() => neighbors;

	public int X { get; }
	public int Y { get; }
	public bool AddNeighbor(QuadEdge edge, QuadFace neighbor) => neighbors.TryAdd(edge, neighbor);

	readonly Dictionary<QuadEdge, Side> edges;
	readonly Dictionary<QuadEdge, QuadFace> neighbors;
}
