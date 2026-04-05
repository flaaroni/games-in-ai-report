using System;
using System.Collections;
using UnityEngine;

public class QuadGridCreator : MonoBehaviour
{
	public class QuadGridEventArgs : EventArgs
	{
		public QuadGridEventArgs(QuadGridCreator source, QuadFace[,] faces)
		{
			Source = source;
			Faces = faces;
		}

		public QuadGridCreator Source { get; }
		public QuadFace[,] Faces { get; }
		public Material[] AllMaterials => Source.grid.AllMaterials;
	}

	public event Action<QuadGridEventArgs> OnMeshGenerated;

	[SerializeField]
	QuadGridMeshes grid;

	[Header("Generator")]
	[SerializeField]
	GameObject groupPrefab;
	[SerializeField]
	GameObject modelPrefab;

	[Header("Debug")]
	[SerializeField]
	[Range(0f, 1f)]
	float delaySeconds = 0f;

	IEnumerator Start()
	{
		if (!grid.IsValid())
		{
			Debug.LogError("Invalid grid configuration. Please check the dimensions and materials.");
			yield break;
		}

		QuadFace[,] faces = grid.GenerateGrid(transform, groupPrefab, modelPrefab);
		if (delaySeconds > float.Epsilon)
		{
			yield return new WaitForSeconds(delaySeconds);
		}
		OnMeshGenerated?.Invoke(new QuadGridEventArgs(this, faces));
	}
}
