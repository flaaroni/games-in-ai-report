using System;
using System.Collections.Generic;

public class QuadEdgeFactory
{
	public QuadEdgeFactory(int maxX, int maxY)
	{
		edgeCache = new Dictionary<Tuple<QuadEdge.Axis, int, int>, QuadEdge>(maxX * maxY);
	}

	public QuadEdge GetEdge(QuadEdge.Axis axis, int x, int y)
	{
		Tuple<QuadEdge.Axis, int, int> key = new(axis, x, y);
		if (!edgeCache.TryGetValue(key, out QuadEdge edge))
		{
			edge = new QuadEdge
			{
				axis = axis,
				X = x,
				Y = y,
			};
			edgeCache.Add(key, edge);
		}
		return edge;
	}

	readonly Dictionary<Tuple<QuadEdge.Axis, int, int>, QuadEdge> edgeCache;
}
