using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
	public enum States : byte
	{
		E,
		I
	}

	[SerializeField]
	QuadGridCreator gridCreator;

	[Header("Debug")]
	[SerializeField]
	[Range(0f, 1f)]
	float delaySeconds = 0f;

	IEnumerable<IFace> faces;
	IEnumerable<IEdge> edges;
	Material[] allMaterials;
	Tuple<List<States>, List<States>>[,] segmentsX;
	Tuple<List<States>, List<States>>[,] segmentsY;

	public Tuple<List<States>, List<States>>[,] SegmentsX => segmentsX;
	public Tuple<List<States>, List<States>>[,] SegmentsY => segmentsX;

	public Tuple<States, States> GetResultX(int x, int y)
	{
		return new(SegmentsX[x, y].Item1[0], SegmentsX[x, y].Item2[0]);
	}
	public Tuple<States, States> GetResultY(int x, int y)
	{
		return new(SegmentsY[x, y].Item1[0], SegmentsY[x, y].Item2[0]);
	}

	void Awake()
	{
		// Bind to the quad grid creator's event
		// this will delay the call until the grid has been generated
		gridCreator.OnMeshGenerated += OnMeshGenerated;
	}

	void OnMeshGenerated(QuadGridCreator.QuadGridEventArgs args)
	{
		// Retrieve necessary data from the event arguments
		faces = args.Faces;
		edges = args.Edges;

		//StartCoroutine(Generate());
	}

	//IEnumerator Generate()
	//{
	//	WaitForSeconds wait = null;
	//	if (delaySeconds > float.Epsilon)
	//	{
	//		wait = new WaitForSeconds(delaySeconds);
	//		yield return wait;
	//	}

	//	// Setup edge data
	//	segmentsX = new Tuple<List<States>, List<States>>[NumCols, NumRows];
	//	segmentsY = new Tuple<List<States>, List<States>>[NumCols, NumRows];
	//	allocateArray(segmentsX);
	//	allocateArray(segmentsY);

	//	// Loop until all faces are collapsed
	//	int row = UnityEngine.Random.Range(1, NumRows);
	//	int col = UnityEngine.Random.Range(1, NumCols);
	//	// 0 = left, 1 = right
	//	int item = UnityEngine.Random.value < 0.5f ? 0 : 1;
	//	// 0 = column, 1 = row
	//	int axis = UnityEngine.Random.value < 0.5f ? 0 : 1;
	//	while (isValidFace(col, row, item, axis))
	//	{
	//		row = UnityEngine.Random.Range(1, NumRows);
	//		col = UnityEngine.Random.Range(1, NumCols);
	//		item = UnityEngine.Random.value < 0.5f ? 0 : 1;
	//		axis = UnityEngine.Random.value < 0.5f ? 0 : 1;

	//		if (wait != null)
	//		{
	//			yield return wait;
	//		}
	//	}
	//	removeState(col, row, item, axis);
	//}

	//bool isValidFace(int col, int row, int item, int axis)
	//{
	//	bool isLowestFace = (axis == 1 && item == 0 && row == 0) || (axis == 0 && item == 0 && col == 0);
	//	bool isHighestFace = (axis == 1 && item == 1 && row == NumRows - 1) || (axis == 0 && item == 1 && col == NumCols - 1);
	//	bool xAndChosen = (axis == 0 && (item == 0 && segmentsX[col, row].Item1.Count == 1) || (item == 1 && segmentsX[col, row].Item2.Count == 2));
	//	bool yAndChosen = (axis == 1 && (item == 0 && segmentsY[col, row].Item1.Count == 1) || (item == 1 && segmentsY[col, row].Item2.Count == 2));
	//	return xAndChosen || yAndChosen || isHighestFace || isLowestFace;
	//}

	//void allocateArray(Tuple<List<States>, List<States>>[,] array)
	//{
	//	array = new Tuple<List<States>, List<States>>[NumCols, NumRows];
	//	for (int i = 0; i < NumCols; i++)
	//	{
	//		for (int j = 0; j < NumRows; j++)
	//		{
	//			array[i, j] = new Tuple<List<States>, List<States>>(new List<States>(), new List<States>());
	//			array[i, j].Item1.Add(States.I);
	//			array[i, j].Item1.Add(States.E);
	//			array[i, j].Item2.Add(States.I);
	//			array[i, j].Item2.Add(States.E);
	//		}
	//	}
	//}

	//void removeState(int col, int row, int item, int axis)
	//{
	//	if (item == 0)
	//	{
	//		if (axis == 0)
	//		{
	//			segmentsX[col, row].Item1.Remove(States.E);
	//		}
	//		else
	//		{
	//			segmentsY[col, row].Item1.Remove(States.E);
	//		}

	//	}
	//	else
	//	{
	//		if (axis == 0)
	//		{
	//			segmentsX[col, row].Item2.Remove(States.E);
	//		}
	//		else
	//		{
	//			segmentsY[col, row].Item2.Remove(States.E);
	//		}
	//	}
	//}
}
