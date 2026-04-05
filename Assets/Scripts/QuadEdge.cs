using System;

public struct QuadEdge : IEquatable<QuadEdge>
{
	public enum Axis : byte { X, Y }

	public Axis axis { get; set; }
	public int X { get; set; }
	public int Y { get; set; }

	public override int GetHashCode() => HashCode.Combine(axis, X, Y);

	public bool Equals(QuadEdge other) =>
		(axis == other.axis)
		&& (X == other.X)
		&& (Y == other.Y);
}
