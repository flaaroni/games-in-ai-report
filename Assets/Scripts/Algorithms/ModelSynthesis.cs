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
		while (unObservedEdges.Count > 0)
		{
			// Choose a random edge to collapse
			IEdge edge = GetRandomEdge();

			// Collapse the edge
			MaterialPair observedPair = CollapseEdge(edge);

			// Recursively propogate the constraints to neighboring edges and faces
			if(!PropogateChanges(edge))
			{
				Reset();
				continue;
			}

			// Confirm all edges and face still has at least 1 possible material
			// , otherwise reset and start over
			foreach (HashSet<MaterialPair> values in edgeToPossibleMaterials.Values)
			{
				if (values.Count == 0)
				{
					Reset();
					continue;
				}
			}
			foreach (HashSet<Material> values in faceToPossibleMaterials.Values)
			{
				if (values.Count == 0)
				{
					Reset();
					continue;
				}
			}

			// FIXME: breaking out of the loop for debugging the first step
			break;
		}
	}

	IEdge GetRandomEdge()
	{
		// First, find the least number of possible materials for any unobserved edge
		int minPossibilities = int.MaxValue;
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

	bool PropogateChanges(IEdge collapseEdge)
	{
		// Setup a list of edges that are already visited
		HashSet<IEdge> visitedEdges = new() { collapseEdge };

		// Setup recursion
		Queue<IEdge> edgeQueue = new();
		edgeQueue.Enqueue(collapseEdge);
		while (edgeQueue.Count > 0)
		{
			// Check the edge
			IEdge edge = edgeQueue.Dequeue();

			// Make sure there are still possibilities for this edge
			var possibilities = edgeToPossibleMaterials[edge];
			if (possibilities.Count == 0)
			{
				return false;
			}

			// Check if there's only one possibility for this edge
			if (possibilities.Count == 1)
			{
				// Remove the edge
				unObservedEdges.Remove(edge);

				// Update the faces of the edge
				var observedPair = possibilities.First();
				CollapseEdgeFace(edge, IEdge.Side.Left, observedPair.left);
				CollapseEdgeFace(edge, IEdge.Side.Right, observedPair.right);

				// Add all the edges from the faces
				foreach (IFace face in edge.Faces.Values)
				{
					foreach (IEdge neighborEdge in face.Edges.Keys)
					{
						if (!visitedEdges.Contains(neighborEdge))
						{
							edgeQueue.Enqueue(neighborEdge);
							visitedEdges.Add(neighborEdge);
						}
					}
				}
				continue;
			}
		}
		return true;
	}

	void CollapseEdgeFace(IEdge edge, IEdge.Side side, Material material)
	{
		// Check if there's a face on the specified side
		if (!edge.Faces.TryGetValue(side, out IFace face))
		{
			return;
		}

		// If so, set the material
		face.Material = material;

		// Update the dictionary of possibilities
		HashSet<Material> faceMaterials = faceToPossibleMaterials[face];
		faceMaterials.Clear();
		faceMaterials.Add(material);
	}
}
