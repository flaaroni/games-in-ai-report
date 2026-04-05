using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
	public enum States{
		I,
		E
	}

	[SerializeField]
	int numCols = 4;
	
	[SerializeField]
	int numRows = 4;

	private Tuple<List<States>, List<States>>[,] segmentsX;
	private Tuple<List<States>, List<States>>[,] segmentsY;
	public Tuple<List<States>, List<States>>[,] Segments => segmentsX;
	public Tuple<States, States> this[int x, int y]
	{
		get
		{
			return new(Segments[x, y].Item1[0], Segments[x, y].Item2[0]);
		}
	}

	void Start()
	{
		segmentsX = new Tuple<List<States>, List<States>>[numCols, numRows];
		segmentsY = new Tuple<List<States>, List<States>>[numCols, numRows];
		allocateArray(segmentsX);
		allocateArray(segmentsY);
		System.Random r = new System.Random();
		int row = r.Next(1, numRows - 1);
		int col = r.Next(1, numCols - 1);
		// 0 = left, 1 = right
		int item = r.Next(2);
		// 0 = column, 1 = row
		int axis = r.Next(2);
		while (isValidFace(col, row, item, axis))
		{
			row = r.Next(1, numRows - 1);
			col = r.Next(1, numCols - 1);
			item = r.Next(2);
			axis = r.Next(2);
		}
		removeState(col, row, item, axis);
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	bool isValidFace(int col, int row, int item, int axis)
	{
		bool isLowestFace = (axis == 1 && item == 0 && row == 0) || (axis == 0 && item == 0 && col == 0);
		bool isHighestFace = (axis == 1 && item == 1 && row == numRows - 1) || (axis == 0 && item == 1 && col == numCols - 1);
		bool xAndChosen = (axis == 0 && (item == 0 && segmentsX[col, row].Item1.Count == 1) || (item == 1 && segmentsX[col, row].Item2.Count == 2));
		bool yAndChosen = (axis == 1 && (item == 0 && segmentsY[col, row].Item1.Count == 1) || (item == 1 && segmentsY[col, row].Item2.Count == 2));
		return xAndChosen || yAndChosen || isHighestFace || isLowestFace;
	}

	void allocateArray(Tuple<List<States>, List<States>>[,] array)
	{
		array = new Tuple<List<States>, List<States>>[numCols, numRows];
		for (int i = 0; i < numCols; i++)
		{
			for (int j = 0; j < numRows; j++)
			{
				array[i, j] = new Tuple<List<States>, List<States>>(new List<States>(), new List<States>());
				array[i, j].Item1.Add(States.I);
				array[i, j].Item1.Add(States.E);
				array[i, j].Item2.Add(States.I);
				array[i, j].Item2.Add(States.E);
			}
		}
	}

	void removeState(int col, int row, int item, int axis)
	{
		if (item == 0)
		{
			if (axis == 0)
			{
				segmentsX[col, row].Item1.Remove(States.E);
			}
			else
			{
				segmentsY[col, row].Item1.Remove(States.E);
			}

		}
		else
		{
			if (axis == 0)
			{
				segmentsX[col, row].Item2.Remove(States.E);
			}
			else
			{
				segmentsY[col, row].Item2.Remove(States.E);
			}
		}
	}
}
