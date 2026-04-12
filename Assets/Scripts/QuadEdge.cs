using System;
using System.Collections.Generic;

public class QuadEdge : IEdge, IEquatable<QuadEdge>
{
	public enum Axis : byte { X, Y }

	public Axis axis { get; set; }
	public int X { get; set; }
	public int Y { get; set; }

	public IDictionary<IEdge.Side, IFace> Faces => throw new NotImplementedException();

	public override int GetHashCode() => HashCode.Combine(axis, X, Y);

	public bool Equals(QuadEdge other) =>
		(axis == other.axis)
		&& (X == other.X)
		&& (Y == other.Y);

	public bool Equals(IEdge other)
	{
		if (other is QuadEdge)
		{
			return Equals((QuadEdge)other);
		}
		return false;
	}
}
