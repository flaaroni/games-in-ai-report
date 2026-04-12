using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFace : IEquatable<IFace>
{
	/// <summary>
	/// Sets the material of this face
	/// </summary>
	public Material Material { set; }
	/// <summary>
	/// Represents the edges creating this face as a dictionary.
	/// Keys indicates the edge index, and the value represents
	/// the left or right side of the edge.
	/// </summary>
	public IReadOnlyDictionary<IEdge, IEdge.Side> Edges { get; }
	/// <summary>
	/// Represents the neighbors as a dictionary.
	/// Keys indicates the edge the neighbors share,
	/// and the value represents the neighbor itself.
	/// </summary>
	public IReadOnlyDictionary<IEdge, IFace> Neighbors { get; }
}
