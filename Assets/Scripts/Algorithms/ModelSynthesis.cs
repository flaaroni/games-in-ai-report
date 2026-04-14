using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSynthesis
{
	// Constraints-related fields
	readonly IReadOnlyList<Material> allMaterials;
	readonly Constraints constraints;

	// Possibility spaces
	readonly HashSet<IFace> unObservedFaces;
	readonly Dictionary<IFace, HashSet<Material>> faceToPossibleMaterials;

	public ModelSynthesis(ICollection<IFace> faces, Constraints constraints)
	{
		// First, compute the constraints
		this.constraints = constraints;
		allMaterials = constraints.GetMaterials();

		// Add all faces with all possible materials to the list
		unObservedFaces = new(faces.Count);
		faceToPossibleMaterials = new(faces.Count);
		foreach (IFace face in faces)
		{
			unObservedFaces.Add(face);
			faceToPossibleMaterials.Add(face, new HashSet<Material>(allMaterials));
		}
	}

	public void Reset()
	{
		// Reset the list of unobserved faces and possible materials for each face
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
			// Choose a random face (with the lowest number of possibilities) to collapse
			IFace face = GetRandomFace();

			// Get all the possibilities for this face
			HashSet<Material> possibleMaterials = faceToPossibleMaterials[face];

			// Choose a random material from the possibilities
			Material observedMaterial = possibleMaterials.ElementAt(Random.Range(0, possibleMaterials.Count));

			// Update the possibility dictionary
			possibleMaterials.Clear();
			possibleMaterials.Add(observedMaterial);

			// Recursively propogate the constraints to neighboring edges and faces
			if (!PropogateChanges(face))
			{
				// If propogating changes failed, restart
				Reset();
				continue;
			}
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
			if (!visitedFaces.Add(checkFace))
			{
				// If we have visited this face, skip it
				continue;
			}

			// Make sure there are still possibilities for this face
			var possibilities = faceToPossibleMaterials[checkFace];
			if (possibilities.Count == 0)
			{
				// If not, indicate failure of propogation
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

			// Check if this is all materials
			if (possibleNeighborMaterials.Count == allMaterials.Count)
			{
				// If so, skip further processing
				// There's no data to propogate to neighbors
				continue;
			}

			// Go through all neighbors
			foreach (IFace neighbor in checkFace.GetNeighbors())
			{
				// If the neighbor is already visited, skip it
				if (!visitedFaces.Add(neighbor))
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
}
