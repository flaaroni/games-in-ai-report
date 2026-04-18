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

	Dictionary<Material, HashSet<Material>> touchingMap;

	public string DisplayName => displayName;

	/// <summary>
	/// Get all materials.
	/// </summary>
	/// <returns>A read-only list of all materials.</returns>
	public IReadOnlyList<Material> GetMaterials()
	{
		HashSet<Material> materials = new HashSet<Material>();
		foreach (var pair in touchingMaterials)
		{
			materials.Add(pair.Left);
			materials.Add(pair.Right);
		}
		return materials.ToArray();
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
		if (touchingMap == null)
		{
			// If not, generate it
			touchingMap = new Dictionary<Material, HashSet<Material>>();

			// Go through all the pairs and add them to the map
			foreach (var pair in touchingMaterials)
			{
				// Check if left material has been added to the map
				if (!touchingMap.TryGetValue(pair.Left, out toReturn))
				{
					// If not, add it with an empty set
					toReturn = new HashSet<Material>();
					touchingMap.Add(pair.Left, toReturn);
				}

				// Add the right material to the left material's set
				toReturn.Add(pair.Right);

				// Check if right material has been added to the map
				if (!touchingMap.TryGetValue(pair.Right, out toReturn))
				{
					// If not, add it with an empty set
					toReturn = new HashSet<Material>();
					touchingMap.Add(pair.Right, toReturn);
				}

				// Add the left material to the right material's set
				toReturn.Add(pair.Left);
			}
		}

		// Check the cache, and return the result if it exists, otherwise return an empty set
		return touchingMap.TryGetValue(material, out toReturn) ? toReturn : new HashSet<Material>();
	}
}
