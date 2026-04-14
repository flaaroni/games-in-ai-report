using System;
using System.Collections.Generic;

public class QuadEdge : IEdge
{
	public enum Axis : byte { X, Y }

	public Axis axis { get; set; }
	public int X { get; set; }
	public int Y { get; set; }

	public IReadOnlyDictionary<IEdge.Side, IFace> Faces => faces;

	public override int GetHashCode() => HashCode.Combine(axis, X, Y);

	public override bool Equals(object other)
	{
		if (other is QuadEdge)
		{
			QuadEdge compare = (QuadEdge)other;
			return (axis == compare.axis)
				&& (X == compare.X)
				&& (Y == compare.Y);
		}
		return false;
	}

	public bool Equals(IEdge other)
	{
		return Equals((object)other);
	}

	public void AddFace(IEdge.Side side, QuadFace face) => faces.TryAdd(side, face);

	readonly Dictionary<IEdge.Side, IFace> faces = new(IEdge.NUM_SIDES);
}
