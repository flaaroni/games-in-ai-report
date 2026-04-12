using System.Collections.Generic;
using UnityEngine;

public class QuadFace : MeshFace
{
	public QuadFace(int x, int y, GameObject model) : base(model)
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

	public override IDictionary<IEdge, IEdge.Side> Edges => edges;

	public override IDictionary<IEdge, IFace> Neighbors => neighbors;

	public int X { get; }
	public int Y { get; }
	public bool AddNeighbor(QuadEdge edge, QuadFace neighbor) => neighbors.TryAdd(edge, neighbor);

	readonly Dictionary<IEdge, IEdge.Side> edges;
	readonly Dictionary<IEdge, IFace> neighbors;
}
