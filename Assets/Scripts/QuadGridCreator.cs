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

	[Header("Grid Size")]
	[SerializeField]
	[Range(1, 10)]
	int xUnits = 3;
	[SerializeField]
	[Range(1, 10)]
	int yUnits = 3;

	[Header("Generator")]
	[SerializeField]
	GameObject groupPrefab;
	[SerializeField]
	GameObject modelPrefab;

	[Header("Metadata")]
	[SerializeField]
	IGridMeshes grid;
	[SerializeField]
	Constraints constraints;

	ModelSynthesis modelSynthesis;

	void Start()
	{
		Generate();
	}

	[ContextMenu("Regenerate")]
	void Generate()
	{
		grid.Setup(xUnits, yUnits);
		if (!grid.IsValid())
		{
			Debug.LogError("Invalid grid configuration. Please check the dimensions and materials.");
			return;
		}

		// Run the generator
		(IEnumerable<IEdge> edges, IEnumerable<IFace> faces) = grid.Generate(transform, groupPrefab, modelPrefab);
		modelSynthesis = new ModelSynthesis(faces, constraints);
		modelSynthesis.Generate();
		OnMeshGenerated?.Invoke(new QuadGridEventArgs(this, edges, faces));
	}

	[ContextMenu("Perform Wave Function Collapse")]
	void WaveFunctionCollapse()
	{
		modelSynthesis.Reset();
		modelSynthesis.Generate();
	}
}
