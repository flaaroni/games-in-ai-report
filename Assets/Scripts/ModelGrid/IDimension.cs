using UnityEngine;

public interface IDimension
{
	/// <summary>
	/// Retrieves the current two-dimensional vector value.
	/// </summary>
	/// <returns>A <see cref="Vector2"/> representing the current vector.</returns>
	Vector2 GetVector(int index);
	/// <summary>
	/// Gets the ray at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the ray to retrieve.</param>
	/// <returns>A Ray representing the ray at the specified index.</returns>
	//Ray GetRay(int index);
}
