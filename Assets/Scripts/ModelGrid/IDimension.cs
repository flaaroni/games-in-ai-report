using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDimension
{
	/// <summary>
	/// Length of the dimension's vector.
	/// </summary>
	[Obsolete("Turn into an array")]
	public float Length
	{
		get;
	}
	/// <summary>
	/// Angle of the dimension's vector in degrees,
	/// where 0 is along the positive x-axis and increases counter-clockwise.
	/// </summary>
	public float Angle
	{
		get;
	}
	/// <summary>
	/// The fraction of the dimension's length at which to subdivide the grid.
	/// </summary>
	[Obsolete("Get rid of subdivisions")]
	public ICollection<float> Subdivisions
	{
		get;
	}
	/// <summary>
	/// Retrieves the current two-dimensional vector value.
	/// </summary>
	/// <returns>A <see cref="Vector2"/> representing the current vector.</returns>
	Vector2 GetVector();
}

public static class IDimensionMethods
{
	public static int NumSubdivisions(this IDimension dimension)
	{
		return dimension.Subdivisions != null ? dimension.Subdivisions.Count : 0;
	}

	public static Vector2 GetVector(this IDimension dimension, int units)
	{
		return dimension.GetVector() * units;
	}

	/// <summary>
	/// Gets a subdivided vector at a specific index based on the dimension's subdivisions.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public static (Vector2 start, Vector2 end) GetSubVector(this IDimension dimension, int index)
	{
		// Setup return values
		Vector2 returnStart = Vector2.zero;
		Vector2 returnEnd = dimension.GetVector();

		// Retrieve necessary data from dimensions
		ICollection<float> subdivisions = dimension.Subdivisions;

		// Make sure arguments are correct
		if (subdivisions == null)
		{
			// Even if subdivisions is null
			// if index is 0, we can still return the full vector
			if (index == 0)
			{
				return (returnStart, returnEnd);
			}

			throw new System.ArgumentOutOfRangeException($"Index {index} is out of range");
		}

		if (index > subdivisions.Count)
		{
			throw new System.ArgumentOutOfRangeException($"Index {index} is out of range");
		}

		// Handle edge case of no subdivisions
		if (subdivisions.Count == 0)
		{
			return (returnStart, returnEnd);
		}

		// FIXME: implement this
		throw new System.NotImplementedException();
	}
}
