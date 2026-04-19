using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadGridCreator : MonoBehaviour
{
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

	public Constraints Constraints { get => constraints; set => constraints = value; }
	public IGridMeshes Grid { get => grid; set => grid = value; }

	IEnumerator Start()
	{
		yield return null; // Wait a frame to ensure all other Start() methods have run
		Generate();
	}

	[ContextMenu("Regenerate")]
	public void Generate()
	{
		if (Grid == null || !Grid.IsValid())
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
		ICollection<IFace> faces = Grid.Generate(transform, groupPrefab, modelPrefab);

		if (faces == null || faces.Count == 0)
		{
			Debug.LogError($"Grid {Grid.GridName} failed to generate any faces");
			return;
		}
		modelSynthesis = new ModelSynthesis(faces, Constraints);
		modelSynthesis.Generate();

		// Generate all faces
		foreach (IFace face in faces)
		{
			face.GenerateMesh();
		}
	}
}
