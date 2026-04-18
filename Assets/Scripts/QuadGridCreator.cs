using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadGridCreator : MonoBehaviour
{
	[Header("Grid Size")]
	[SerializeField]
	[Range(1, 50)]
	int xUnits = 3;
	[SerializeField]
	[Range(1, 50)]
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
	public void Generate()
	{
		grid.Setup(xUnits, yUnits);
		if (!grid.IsValid())
		{
			Debug.LogError("Invalid grid configuration. Please check the dimensions and materials.");
			return;
		}

		// Destroy all prior game objects
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}

		// Run the generator
		ICollection<IFace> faces = grid.Generate(transform, groupPrefab, modelPrefab);
		modelSynthesis = new ModelSynthesis(faces, constraints);
		modelSynthesis.Generate();

		// Generate all faces
		foreach (IFace face in faces)
		{
			face.GenerateMesh();
		}
	}
}
