using System.Collections.Generic;
using UnityEngine;

public class QuadFace : IFace
{
	public QuadFace(GameObject model, int x, int y)
	{
		// Setup member variables
		X = x;
		Y = y;
		renderer = model.GetComponent<MeshRenderer>();

		// Generate edges from a factory and add them to the dictionary
		neighbors = new HashSet<QuadFace>(4);
	}

	public int X { get; }
	public int Y { get; }

	public Material Material
	{
		get => renderer.sharedMaterial;
		set => renderer.sharedMaterial = value;
	}

	public void AddNeighbor(QuadFace face)
	{
		neighbors.Add(face);
		face.neighbors.Add(this);
	}

	public IEnumerable<IFace> GetNeighbors()
	{
		foreach (QuadFace toReturn in neighbors)
		{
			yield return toReturn;
		}
	}

	public override int GetHashCode() => renderer.GetHashCode();

	public override bool Equals(object other)
	{
		if (other is QuadFace)
		{
			return renderer == ((QuadFace)other).renderer;
		}
		return false;
	}

	// Member variables
	readonly MeshRenderer renderer;
	readonly ISet<QuadFace> neighbors;
}
