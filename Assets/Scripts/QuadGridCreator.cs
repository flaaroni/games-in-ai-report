using System;
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
	}

	public event Action<QuadGridEventArgs> OnMeshGenerated;

	[SerializeField]
	QuadGridMeshes grid;

	[Header("Generator")]
	[SerializeField]
	GameObject groupPrefab;
	[SerializeField]
	GameObject modelPrefab;

	void Start()
	{
		if (!grid.IsValid())
		{
			Debug.LogError("Invalid grid configuration. Please check the dimensions and materials.");
			return;
		}
		QuadFace[,] faces = grid.GenerateGrid(transform, groupPrefab, modelPrefab);
		OnMeshGenerated?.Invoke(new QuadGridEventArgs(this, faces));
	}
}
