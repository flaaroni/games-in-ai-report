using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Constraints.asset", menuName = "AI in Games/Constraints", order = 3)]
public class Constraints : ScriptableObject
{
	[Serializable]
	public struct MaterialPair
	{
		[SerializeField]
		Material left;
		[SerializeField]
		Material right;

		public Material Left => left;
		public Material Right => right;
	}

	[SerializeField]
	string displayName;
	[SerializeField]
	MaterialPair[] touchingMaterials;

	// All unique materials that appear in the touchingMaterials array, generated on demand
	Material[] allMaterials = null;
	// Cache for the map of materials to the materials they are touching, generated on demand
	// Index is the index of the material in the allMaterials array,
	// value is the set of materials that are touching that material
	Dictionary<byte, HashSet<Material>> touchingMap = null;

	public string DisplayName => displayName;

	/// <summary>
	/// Get all materials.
	/// </summary>
	/// <returns>A read-only list of all materials.</returns>
	public IReadOnlyList<Material> GetMaterials()
	{
		if ((allMaterials == null) || (allMaterials.Length == 0))
		{
			HashSet<Material> materials = new HashSet<Material>();
			foreach (var pair in touchingMaterials)
			{
				materials.Add(pair.Left);
				materials.Add(pair.Right);
			}
			allMaterials = materials.ToArray();
		}
		return allMaterials;
	}

	/// <summary>
	/// Get a set of materials that are touching the given material.
	/// </summary>
	/// <param name="material">The material to check for touching materials.</param>
	/// <returns>A set of materials that are touching the given material.</returns>
	public ISet<Material> GetTouchingMaterials(Material material)
	{
		HashSet<Material> toReturn;

		// Check if the map has been generated yet
		byte index;
		if (touchingMap == null)
		{
			// If not, generate it
			touchingMap = new(GetMaterials().Count);

			// Go through all the pairs and add them to the map
			foreach (var pair in touchingMaterials)
			{
				// Check if left material has been added to the map
				index = GetMaterialIndex(pair.Left);
				if (!touchingMap.TryGetValue(index, out toReturn))
				{
					// If not, add it with an empty set
					toReturn = new HashSet<Material>();
					touchingMap.Add(index, toReturn);
				}

				// Add the right material to the left material's set
				toReturn.Add(pair.Right);

				// Check if right material has been added to the map
				index = GetMaterialIndex(pair.Right);
				if (!touchingMap.TryGetValue(index, out toReturn))
				{
					// If not, add it with an empty set
					toReturn = new HashSet<Material>();
					touchingMap.Add(index, toReturn);
				}

				// Add the left material to the right material's set
				toReturn.Add(pair.Left);
			}
		}

		// Check the cache, and return the result if it exists, otherwise return an empty set
		index = GetMaterialIndex(material);
		return touchingMap.TryGetValue(index, out toReturn) ? toReturn : new HashSet<Material>();
	}

	private byte GetMaterialIndex(Material material)
	{
		for (byte i = 0; i < allMaterials.Length; i++)
		{
			if (allMaterials[i] == material)
			{
				return i;
			}
		}
		throw new ArgumentException($"Material {material} not found in constraints");
	}
}
