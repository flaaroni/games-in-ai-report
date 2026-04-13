using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridGenerator
{
	public bool IsValid();
	public (IEnumerable<IEdge> edges, IEnumerable<IFace> faces) Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab);
}
