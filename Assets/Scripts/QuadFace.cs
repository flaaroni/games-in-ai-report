using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadFace : MeshFace<Vector2Int, QuadFace>
{
	public QuadFace(int x, int y, GameObject model, byte numMaterials) : base(model, numMaterials)
	{
		X = x;
		Y = y;
	}

	public override IDictionary<Vector2Int, bool> Edges => throw new NotImplementedException();

	public override IEnumerable<QuadFace> GetNeighbors()
	{
		throw new NotImplementedException();
	}

	public int X { get; }
	public int Y { get; }
}
