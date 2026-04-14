using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ModelSynthesis
{
	// Constraints-related fields
	readonly IReadOnlyList<Material> allMaterials;
	readonly Constraints constraints;

	// Possibility spaces
	readonly SortedSet<IEdge> unObservedEdges;
	readonly Dictionary<IEdge, HashSet<Material>> edgeToPossibleMaterials;
	readonly Dictionary<IFace, HashSet<Material>> faceToPossibleMaterials;

	public ModelSynthesis(IEnumerable<IEdge> edges, IEnumerable<IFace> faces, Constraints constraints)
	{
		// First, compute the constraints
		this.constraints = constraints;
		allMaterials = constraints.GetMaterials();

		// Add all edges with all possible materials to the list
		unObservedEdges = new();
		edgeToPossibleMaterials = new();
		foreach (IEdge edge in edges)
		{
			unObservedEdges.Add(edge);
			edgeToPossibleMaterials.Add(edge, new HashSet<Material>(allMaterials));
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
		foreach ((IEdge edge, HashSet<Material> materials) in edgeToPossibleMaterials)
		{
			unObservedEdges.Add(edge);
			materials.UnionWith(allMaterials);
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
			isGenerating = false;
		}
		while(isGenerating);
	}
}
