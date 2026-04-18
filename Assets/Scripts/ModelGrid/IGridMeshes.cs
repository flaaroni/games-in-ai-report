using System.Collections.Generic;
using UnityEngine;

public abstract class IGridMeshes : ScriptableObject, IGridGenerator
{
	public virtual string GridName => name;
	public virtual Vector3 CameraPosition => Vector3.zero;
	public abstract void Setup(params int[] numUnitsPerAxis);
	public abstract bool IsValid();
	public abstract ICollection<IFace> Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab);
}
