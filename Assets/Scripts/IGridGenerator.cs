using System.Collections.Generic;
using UnityEngine;

public interface IGridGenerator
{
	/// <summary>
	/// Checks if the current settings of this grid generator are valid for generating a grid.
	/// </summary>
	/// <returns></returns>
	public bool IsValid();
	/// <summary>
	/// Generates a grid with a list of edges and faces.
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="groupPrefab"></param>
	/// <param name="modelPrefab"></param>
	/// <returns></returns>
	public (IEnumerable<IEdge> edges, IEnumerable<IFace> faces) Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab);
}
