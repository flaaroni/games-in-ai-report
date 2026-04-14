using System.Collections.Generic;
using UnityEngine;

public interface IGridGenerator
{
	/// <summary>
	/// Sets the number of units on each axis of the grid.
	/// The number of parameters should match the number of dimensions of the grid.
	/// </summary>
	/// <param name="numUnitsPerAxis"></param>
	public void Setup(params int[] numUnitsPerAxis);
	/// <summary>
	/// Checks if the current settings of this grid generator are valid for generating a grid.
	/// </summary>
	/// <returns></returns>
	public bool IsValid();
	/// <summary>
	/// Generates a grid with an iterator of faces.
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="groupPrefab"></param>
	/// <param name="modelPrefab"></param>
	/// <returns></returns>
	public IEnumerable<IFace> Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab);
}
