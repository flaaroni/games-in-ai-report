using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PolygonCell
{
	[SerializeField]
	private List<Vector2> vertices = new List<Vector2>();

	[SerializeField]
	private List<int> neighborIndices = new List<int>();

	public List<Vector2> Vertices => vertices;
	public List<int> NeighborIndices => neighborIndices;

	public PolygonCell()
	{
	}

	public PolygonCell(IEnumerable<Vector2> vertices)
	{
		this.vertices = new List<Vector2>(vertices);
	}

	public float SignedArea()
	{
		float area = 0f;
		int count = vertices.Count;

		for (int i = 0; i < count; ++i)
		{
			Vector2 a = vertices[i];
			Vector2 b = vertices[(i + 1) % count];
			area += (a.x * b.y - b.x * a.y);
		}

		return 0.5f * area;
	}

	public float Area()
	{
		return Mathf.Abs(SignedArea());
	}

	public bool IsClockwise()
	{
		return SignedArea() < 0f;
	}

	public void EnsureClockwise()
	{
		if (!IsClockwise())
		{
			vertices.Reverse();
		}
	}

	public Vector2 Centroid()
	{
		if (vertices.Count == 0)
		{
			return Vector2.zero;
		}

		float area = SignedArea();

		if (Mathf.Abs(area) < 1e-6f)
		{
			Vector2 avg = Vector2.zero;
			for (int i = 0; i < vertices.Count; ++i)
			{
				avg += vertices[i];
			}
			return avg / vertices.Count;
		}

		float cx = 0f;
		float cy = 0f;

		for (int i = 0; i < vertices.Count; ++i)
		{
			Vector2 a = vertices[i];
			Vector2 b = vertices[(i + 1) % vertices.Count];
			float cross = a.x * b.y - b.x * a.y;
			cx += (a.x + b.x) * cross;
			cy += (a.y + b.y) * cross;
		}

		float scale = 1f / (6f * area);
		return new Vector2(cx * scale, cy * scale);
	}
}