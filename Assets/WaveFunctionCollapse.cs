using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
	public enum States{
		I,
		E
	}

	[SerializeField]
	int numCols = 3;
	
	[SerializeField]
	int numRows = 3;

	private Tuple<List<States>, List<States>>[,] segments;

	public Tuple<List<States>, List<States>>[,] Segments => segments;

	void Start()
    {
        segments = new Tuple<List<States>, List<States>>[numCols, numRows];
		for(int i = 0; i < numCols; i++)
		{
			for(int j  = 0; j < numRows; j++)
			{
				segments[i, j] = new Tuple<List<States>, List<States>>(new List<States>(), new List<States>());
				segments[i, j].Item1.Add(States.I);
				segments[i, j].Item1.Add(States.E);
				segments[i, j].Item2.Add(States.I);
				segments[i, j].Item2.Add(States.E);
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
