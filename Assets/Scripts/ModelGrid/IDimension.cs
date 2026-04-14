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
	/// Retrieves the current two-dimensional vector value.
	/// </summary>
	/// <returns>A <see cref="Vector2"/> representing the current vector.</returns>
	Vector2 GetVector();
}

public static class IDimensionMethods
{
	public static Vector2 GetVector(this IDimension dimension, int units)
	{
		return dimension.GetVector() * units;
	}
}
