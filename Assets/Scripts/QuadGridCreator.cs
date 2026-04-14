using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadGridCreator : MonoBehaviour
{
	public class QuadGridEventArgs : EventArgs
	{
		public QuadGridEventArgs(QuadGridCreator source, IEnumerable<IEdge> edges, IEnumerable<IFace> faces)
		{
			Source = source;
			Edges = edges;
			Faces = faces;
		}

		public QuadGridCreator Source { get; }
		public IEnumerable<IEdge> Edges { get; }
		public IEnumerable<IFace> Faces { get; }
	}

	public event Action<QuadGridEventArgs> OnMeshGenerated;

	[SerializeField]
	QuadGridMeshes grid;

	[Header("Generator")]
	[SerializeField]
	GameObject groupPrefab;
	[SerializeField]
	GameObject modelPrefab;

	IEnumerator Start()
	{
		if (!grid.IsValid())
		{
			Debug.LogError("Invalid grid configuration. Please check the dimensions and materials.");
			yield break;
		}

		// Run the generator
		(IEnumerable<IEdge> edges, IEnumerable<IFace> faces) = grid.Generate(transform, groupPrefab, modelPrefab);
		OnMeshGenerated?.Invoke(new QuadGridEventArgs(this, edges, faces));
	}
}
