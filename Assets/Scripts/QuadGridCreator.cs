using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadGridCreator : MonoBehaviour
{
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
	}

	[ContextMenu("Perform Wave Function Collapse")]
	void WaveFunctionCollapse()
	{
		modelSynthesis.Reset();
		modelSynthesis.Generate();
	}
}
