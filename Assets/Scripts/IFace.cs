using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFace : IEquatable<object>
{
	/// <summary>
	/// Gets or sets the material associated with this object.
	/// </summary>
	public Material Material { get; set; }
	/// <summary>
	/// Gets all neighbors of this face, where a neighbor is defined as a face that shares an edge with this face.
	/// </summary>
	/// <returns>An iterator of faces.</returns>
	public IEnumerable<IFace> GetNeighbors();
	/// <summary>
	/// Required so this face can be used as a key in a dictionary, and to compare two faces.
	/// </summary>
	/// <returns></returns>
	public int GetHashCode();
}
