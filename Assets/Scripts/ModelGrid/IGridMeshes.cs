using System.Collections.Generic;
using UnityEngine;

public abstract class IGridMeshes : ScriptableObject, IGridGenerator
{
	public abstract void Setup(params int[] numUnitsPerAxis);
	public abstract bool IsValid();
	public abstract (IEnumerable<IEdge> edges, IEnumerable<IFace> faces) Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab);
}
