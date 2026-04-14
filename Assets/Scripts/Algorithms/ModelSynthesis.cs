using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSynthesis
{
	class MaterialPair
	{
		public Material left;
		public Material right;
	}

	// Constraints-related fields
	readonly IReadOnlyList<Material> allMaterials;
	readonly List<MaterialPair> allValidPairs;
	readonly Constraints constraints;

	// Possibility spaces
	readonly HashSet<IEdge> unObservedEdges;
	readonly Dictionary<IEdge, HashSet<MaterialPair>> edgeToPossibleMaterials;
	readonly Dictionary<IFace, HashSet<Material>> faceToPossibleMaterials;

	public ModelSynthesis(IEnumerable<IEdge> edges, IEnumerable<IFace> faces, Constraints constraints)
	{
		// First, compute the constraints
		this.constraints = constraints;
		allMaterials = constraints.GetMaterials();

		// Compute all the valid pairs of materials for edges
		allValidPairs = new();
		foreach (Material left in allMaterials)
		{
			ISet<Material> rightMaterials = constraints.GetTouchingMaterials(left);
			foreach (Material right in rightMaterials)
			{
				allValidPairs.Add(new MaterialPair() { left = left, right = right });
			}
		}

		// Add all edges with all possible materials to the list
		unObservedEdges = new();
		edgeToPossibleMaterials = new();
		foreach (IEdge edge in edges)
		{
			unObservedEdges.Add(edge);
			edgeToPossibleMaterials.Add(edge, new HashSet<MaterialPair>(allValidPairs));
		}

		// Add all faces with all possible materials to the list
		faceToPossibleMaterials = new();
		foreach (IFace face in faces)
		{
			faceToPossibleMaterials.Add(face, new HashSet<Material>(allMaterials));
		}
	}

	public void Reset()
	{
		// Reset the list of unobserved edges and possible materials for each edge and face
		unObservedEdges.Clear();
		foreach ((IEdge edge, HashSet<MaterialPair> materials) in edgeToPossibleMaterials)
		{
			unObservedEdges.Add(edge);
			materials.UnionWith(allValidPairs);
		}
		foreach ((IFace face, HashSet<Material> materials) in faceToPossibleMaterials)
		{
			materials.UnionWith(allMaterials);
		}
	}

	public void Generate()
	{
		bool isGenerating = true;
		do
		{
			// Choose a random edge to collapse
			IEdge edge = GetRandomEdge();

			// FIXME: collapse the edge
			MaterialPair observedPair = CollapseEdge(edge);

			// Remove the edge
			unObservedEdges.Remove(edge);

			// FIXME: update the faces of the edge
			// FIXME: recursively propogate the constraints to neighboring edges and faces

			// FIXME: confirm all edges and face still has at least 1 possible material
			// , otherwise reset and start over
			isGenerating = false;
		}
		while (isGenerating);
	}

	IEdge GetRandomEdge()
	{
		// First, find the least number of possible materials for any unobserved edge
		int minPossibilities = allMaterials.Count;
		foreach (IEdge edge in unObservedEdges)
		{
			int numPossibilities = edgeToPossibleMaterials[edge].Count;
			if (numPossibilities < minPossibilities)
			{
				minPossibilities = numPossibilities;
			}
		}

		// Return any edges that meets this criteria
		var toReturn = new List<IEdge>(unObservedEdges.Count);
		foreach (IEdge edge in unObservedEdges)
		{
			if (minPossibilities == edgeToPossibleMaterials[edge].Count)
			{
				toReturn.Add(edge);
			}
		}
		return toReturn[Random.Range(0, toReturn.Count)];
	}

	MaterialPair CollapseEdge(IEdge edge)
	{
		// Get all the possibilities for this edge
		HashSet<MaterialPair> possiblePairs = edgeToPossibleMaterials[edge];

		// Choose a random pair from the possibilities
		MaterialPair observedPair = possiblePairs.ElementAt(Random.Range(0, possiblePairs.Count));

		// Update the possibility dictionary
		possiblePairs.Clear();
		possiblePairs.Add(observedPair);

		// Return the observed pair
		return observedPair;
	}
}
