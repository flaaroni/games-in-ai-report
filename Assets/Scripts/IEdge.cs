using System;
using System.Collections.Generic;

public interface IEdge : IEquatable<IEdge>, IEquatable<object>
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
	public IReadOnlyDictionary<Side, IFace> Faces { get; }
	/// <summary>
	/// Required so this face can be used as a key in a dictionary, and to compare two faces.
	/// </summary>
	/// <returns></returns>
	public int GetHashCode();
}
