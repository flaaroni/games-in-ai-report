using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSynthesis
{
	//class MaterialPair
	//{
	//	public Material left;
	//	public Material right;
	//}

	// Constraints-related fields
	readonly IReadOnlyList<Material> allMaterials;
	//readonly List<MaterialPair> allValidPairs;
	readonly Constraints constraints;

	// Possibility spaces
	//readonly HashSet<IEdge> unObservedEdges;
	//readonly Dictionary<IEdge, HashSet<MaterialPair>> edgeToPossibleMaterials;
	readonly HashSet<IFace> unObservedFaces;
	readonly Dictionary<IFace, HashSet<Material>> faceToPossibleMaterials;

	public ModelSynthesis(IEnumerable<IEdge> edges, IEnumerable<IFace> faces, Constraints constraints)
	{
		// First, compute the constraints
		this.constraints = constraints;
		allMaterials = constraints.GetMaterials();

		// Compute all the valid pairs of materials for edges
		//allValidPairs = new();
		//foreach (Material left in allMaterials)
		//{
		//	ISet<Material> rightMaterials = constraints.GetTouchingMaterials(left);
		//	foreach (Material right in rightMaterials)
		//	{
		//		allValidPairs.Add(new MaterialPair() { left = left, right = right });
		//	}
		//}

		// Add all edges with all possible materials to the list
		//unObservedEdges = new();
		//edgeToPossibleMaterials = new();
		//foreach (IEdge edge in edges)
		//{
		//	unObservedEdges.Add(edge);
		//	edgeToPossibleMaterials.Add(edge, new HashSet<MaterialPair>(allValidPairs));
		//}

		// Add all faces with all possible materials to the list
		unObservedFaces = new();
		faceToPossibleMaterials = new();
		foreach (IFace face in faces)
		{
			unObservedFaces.Add(face);
			faceToPossibleMaterials.Add(face, new HashSet<Material>(allMaterials));
		}
	}

	public void Reset()
	{
		// Reset the list of unobserved edges and possible materials for each edge and face
		//unObservedEdges.Clear();
		//foreach ((IEdge edge, HashSet<MaterialPair> materials) in edgeToPossibleMaterials)
		//{
		//	unObservedEdges.Add(edge);
		//	materials.UnionWith(allValidPairs);
		//}
		unObservedFaces.Clear();
		foreach ((IFace face, HashSet<Material> materials) in faceToPossibleMaterials)
		{
			unObservedFaces.Add(face);
			materials.UnionWith(allMaterials);
		}
	}

	public void Generate()
	{
		while (unObservedFaces.Count > 0)
		{
			// Choose a random edge to collapse
			IFace face = GetRandomFace();

			// Collapse the edge
			CollapseFace(face);

			// Recursively propogate the constraints to neighboring edges and faces
			if(!PropogateChanges(face))
			{
				// If propogating changes failed, restart
				Reset();
				continue;
			}

			// FIXME: breaking out of the loop for debugging the first step
			break;
		}
	}

	IFace GetRandomFace()
	{
		// First, find the least number of possible materials for any unobserved edge
		int minPossibilities = int.MaxValue;
		foreach (IFace face in unObservedFaces)
		{
			int numPossibilities = faceToPossibleMaterials[face].Count;
			if (numPossibilities < minPossibilities)
			{
				minPossibilities = numPossibilities;
			}
		}

		// Return any edges that meets this criteria
		var toReturn = new List<IFace>(unObservedFaces.Count);
		foreach (IFace face in unObservedFaces)
		{
			if (minPossibilities == faceToPossibleMaterials[face].Count)
			{
				toReturn.Add(face);
			}
		}
		return toReturn[Random.Range(0, toReturn.Count)];
	}

	Material CollapseFace(IFace face)
	{
		// Get all the possibilities for this edge
		HashSet<Material> possibleMaterials = faceToPossibleMaterials[face];

		// Choose a random material from the possibilities
		Material observedMaterial = possibleMaterials.ElementAt(Random.Range(0, possibleMaterials.Count));

		// Update the possibility dictionary
		possibleMaterials.Clear();
		possibleMaterials.Add(observedMaterial);

		// Return the observed material
		return observedMaterial;
	}

	bool PropogateChanges(IFace face)
	{
		// Setup a list of edges that are already visited
		HashSet<IFace> visitedFaces = new();

		// Setup recursion
		Queue<IFace> faceQueue = new();
		faceQueue.Enqueue(face);
		while (faceQueue.Count > 0)
		{
			// Dequeue, and make sure we haven't visited this face, yet
			IFace checkFace = faceQueue.Dequeue();
			if (visitedFaces.Contains(checkFace))
			{
				// If we have visited this face, skip it
				continue;
			}

			// If not, mark this face as visited
			visitedFaces.Add(checkFace);

			// Make sure there are still possibilities for this face
			var possibilities = faceToPossibleMaterials[checkFace];
			if (possibilities.Count == 0)
			{
				// If not, halt immediately
				// Indicate failure of propogation
				return false;
			}

			// Check if there's only one possibility for this face
			if (possibilities.Count == 1)
			{
				// Set the face's material to the only possibility
				checkFace.Material = possibilities.First();

				// Remove the face from the list of unobserved faces
				unObservedFaces.Remove(checkFace);
			}

			// Make a list of possibilities for this face's neighbors
			HashSet<Material> possibleNeighborMaterials = new();
			foreach (Material material in possibilities)
			{
				possibleNeighborMaterials.UnionWith(constraints.GetTouchingMaterials(material));
			}

			// Go through all neighbors
			foreach(IFace neighbor in checkFace.GetNeighbors())
			{
				// If the neighbor is already visited, skip it
				if (visitedFaces.Contains(neighbor))
				{
					continue;
				}

				// If not, update the possibilities for this neighbor by intersecting with the possible neighbor materials
				faceToPossibleMaterials[neighbor].IntersectWith(possibleNeighborMaterials);

				// Add the neighbor to the queue
				faceQueue.Enqueue(neighbor);
			}
		}
		return true;
	}

	//void CollapseEdgeFace(IEdge edge, IEdge.Side side, Material material)
	//{
	//	// Check if there's a face on the specified side
	//	if (!edge.Faces.TryGetValue(side, out IFace face))
	//	{
	//		return;
	//	}

	//	// If so, set the material
	//	face.Material = material;

	//	// Update the dictionary of possibilities
	//	HashSet<Material> faceMaterials = faceToPossibleMaterials[face];
	//	faceMaterials.Clear();
	//	faceMaterials.Add(material);
	//}
}
