using UnityEngine;

[System.Serializable]
public struct Line2D
{
	[SerializeField]
	Vector2 normal;

	[SerializeField]
	float offset;

	public Vector2 Normal => normal;
	public float Offset => offset;

	public Line2D(Vector2 normal, float offset)
	{
		if (normal.sqrMagnitude < 1e-8f)
		{
			throw new System.ArgumentException("Line normal cannot be zero.");
		}

		this.normal = normal.normalized;
		this.offset = offset;
	}

	public static Line2D FromPointAndDirection(Vector2 point, Vector2 direction)
	{
		if (direction.sqrMagnitude < 1e-8f)
		{
			throw new System.ArgumentException("Line direction cannot be zero.");
		}

		Vector2 dir = direction.normalized;
		Vector2 n = new Vector2(-dir.y, dir.x); // perpendicular
		float d = Vector2.Dot(n, point);
		return new Line2D(n, d);
	}

	public static Line2D FromPointAndNormal(Vector2 point, Vector2 normal)
	{
		if (normal.sqrMagnitude < 1e-8f)
		{
			throw new System.ArgumentException("Line normal cannot be zero.");
		}

		Vector2 n = normal.normalized;
		float d = Vector2.Dot(n, point);
		return new Line2D(n, d);
	}

	public float SignedDistance(Vector2 point)
	{
		return Vector2.Dot(normal, point) - offset;
	}

	public Vector2 ProjectPoint(Vector2 point)
	{
		float dist = SignedDistance(point);
		return point - dist * normal;
	}

	public Vector2 Direction
	{
		get
		{
			return new Vector2(normal.y, -normal.x);
		}
	}

	public bool TryIntersect(Line2D other, out Vector2 intersection, float epsilon = 1e-6f)
	{
		float det = normal.x * other.normal.y - normal.y * other.normal.x;

		if (Mathf.Abs(det) < epsilon)
		{
			intersection = default;
			return false;
		}

		intersection = new Vector2(
			(offset * other.normal.y - normal.y * other.offset) / det,
			(normal.x * other.offset - offset * other.normal.x) / det
		);
		return true;
	}
}