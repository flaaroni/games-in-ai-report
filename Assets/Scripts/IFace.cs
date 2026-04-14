using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFace : IEquatable<IFace>, IEquatable<object>
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
	/// Gets a neighbor for a given edge.
	/// </summary>
	/// <param name="edge">The edge to get the neighbor for.</param>
	/// <returns>The neighboring face, or null if there isn't one.</returns>
	public IFace GetNeighbor(IEdge edge);
	/// <summary>
	/// Required so this face can be used as a key in a dictionary, and to compare two faces.
	/// </summary>
	/// <returns></returns>
	public int GetHashCode();
}
