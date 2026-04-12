using System;
using System.Collections.Generic;

public interface IEdge : IEquatable<IEdge>
{
	/// <summary>
	/// Assuming the edge is like a vector,
	/// with start and end point, indicates the side of the edge,
	/// where a face *could* be.
	/// </summary>
	public enum Side : byte { Left, Right }
	/// <summary>
	/// Represents the faces this edge belongs to as a dictionary.
	/// </summary>
	public IDictionary<Side, IFace> Faces { get; }
}
