using UnityEngine;

[System.Serializable]
public struct Segment2D
{
	[SerializeField]
	private Vector2 a;

	[SerializeField]
	private Vector2 b;

	public Vector2 A => a;
	public Vector2 B => b;

	public Vector2 Midpoint => 0.5f * (a + b);
	public Vector2 Delta => b - a;
	public float Length => Delta.magnitude;

	public Segment2D(Vector2 a, Vector2 b)
	{
		this.a = a;
		this.b = b;
	}

	public Vector2 GetPoint(float t)
	{
		return Vector2.Lerp(a, b, Mathf.Clamp01(t));
	}

	public bool ContainsPoint(Vector2 p, float epsilon = 1e-5f)
	{
		Vector2 ap = p - a;
		Vector2 ab = b - a;

		float cross = ab.x * ap.y - ab.y * ap.x;
		if (Mathf.Abs(cross) > epsilon)
		{
			return false;
		}

		float dot = Vector2.Dot(ap, ab);
		if (dot < -epsilon)
		{
			return false;
		}

		float abLenSq = ab.sqrMagnitude;
		if (dot > abLenSq + epsilon)
		{
			return false;
		}

		return true;
	}

	public override string ToString()
	{
		return $"Segment2D({a} -> {b})";
	}
}