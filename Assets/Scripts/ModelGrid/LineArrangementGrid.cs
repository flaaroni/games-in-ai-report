using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LineArrangementGrid.asset", menuName = "AI in Games/Line Arrangement Grid", order = 4)]
public class LineArrangementGrid : IGridMeshes
{
	[Serializable]
	public class LineFamily
	{
		[SerializeField]
		private float angleDegrees = 0f;
		
		[SerializeField]
		private float startOffset = 0f;

		[SerializeField]
		[Min(1)]
		private int count = 2;

		[SerializeField]
		private float[] spacings = new float[] { 1f };

		public float AngleDegrees => angleDegrees;
		public float StartOffset => startOffset;
		public int Count => Mathf.Max(1, count);
		public IReadOnlyList<float> Spacings => spacings;

		public bool IsValid()
		{
			if (count < 1) return false;
			if (spacings == null || spacings.Length == 0) return false;

			for (int i = 0; i < spacings.Length; ++i)
			{
				if (spacings[i] <= 0f) return false;
			}

			return true;
		}
	}

	private class FamilyData
	{
		public Vector2 Normal;
		public float[] Offsets;
	}

	private class CellRecord
	{
		public int[] IntervalIndices;
		public PolygonCell Polygon;
		public MeshFace Face;
	}

	[Header("Boundary Rectangle")]
	
	[SerializeField]
	private string gridname = " ";
	[SerializeField]
	private Vector3 cameraPosition = new Vector3();

	[SerializeField]
	private Vector2 boundaryMin = new Vector2(-5f, -5f);

	[SerializeField]
	private Vector2 boundaryMax = new Vector2(5f, 5f);

	[Header("Line Families")]
	[SerializeField]
	private LineFamily[] families = Array.Empty<LineFamily>();

	[Header("Debug")]
	[SerializeField]
	private bool drawDebugSegments = true;

	private readonly List<Line2D> builtLines = new();
	private readonly List<Segment2D> clippedSegments = new();

	public IReadOnlyList<Line2D> BuiltLines => builtLines;
	public IReadOnlyList<Segment2D> ClippedSegments => clippedSegments;

	private const float Epsilon = 1e-4f;
	private const float AreaEpsilon = 1e-3f;

	public override string GridName => gridname;
	public override Vector3 CameraPosition => cameraPosition;
	public override void Setup(params int[] numUnitsPerAxis)
	{
		// Not used. This generator is driven by line families.
	}

	public override bool IsValid()
	{
		if (boundaryMax.x <= boundaryMin.x || boundaryMax.y <= boundaryMin.y)
			return false;

		if (families == null || families.Length == 0)
			return false;

		for (int i = 0; i < families.Length; ++i)
		{
			if (families[i] == null || !families[i].IsValid())
				return false;
		}

		return true;
	}

	public override ICollection<IFace> Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab)
	{
		builtLines.Clear();
		clippedSegments.Clear();

		BuildLines(builtLines);
		ClipLinesToBoundary(builtLines, clippedSegments);

		//Debug.Log($"Built {builtLines.Count} lines and {clippedSegments.Count} clipped boundary segments.");

		return BuildFacesBySlabIntersection(parent, groupPrefab, modelPrefab);
	}

	private ICollection<IFace> BuildFacesBySlabIntersection(
		Transform parent,
		GameObject groupPrefab,
		GameObject modelPrefab)
	{
		List<IFace> faces = new();

		FamilyData[] familyData = BuildFamilyData();
		if (familyData.Length == 0)
			return faces;

		for (int i = 0; i < familyData.Length; ++i)
		{
			if (familyData[i].Offsets.Length < 2)
			{
				Debug.LogWarning($"Family {i} needs at least 2 lines to form cells.");
				return faces;
			}
		}

		List<CellRecord> records = new();
		Dictionary<string, CellRecord> keyToRecord = new();

		int[] intervals = new int[familyData.Length];
		EnumerateCellsRecursive(
			0,
			intervals,
			familyData,
			parent,
			groupPrefab,
			modelPrefab,
			faces,
			records,
			keyToRecord);

		AssignNeighborsByIntervals(records, keyToRecord);

		//Debug.Log($"Created {faces.Count} faces.");
		return faces;
	}

	private FamilyData[] BuildFamilyData()
	{
		FamilyData[] result = new FamilyData[families.Length];

		for (int i = 0; i < families.Length; ++i)
		{
			LineFamily family = families[i];

			float angleRad = family.AngleDegrees * Mathf.Deg2Rad;
			Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
			Vector2 normal = new Vector2(-direction.y, direction.x).normalized;

			float[] offsets = new float[family.Count];
			float currentOffset = family.StartOffset;

			for (int j = 0; j < family.Count; ++j)
			{
				offsets[j] = currentOffset;
				currentOffset += family.Spacings[j % family.Spacings.Count];
			}

			result[i] = new FamilyData
			{
				Normal = normal,
				Offsets = offsets
			};
		}

		return result;
	}

	private void EnumerateCellsRecursive(
		int familyIndex,
		int[] intervals,
		FamilyData[] familyData,
		Transform parent,
		GameObject groupPrefab,
		GameObject modelPrefab,
		List<IFace> faces,
		List<CellRecord> records,
		Dictionary<string, CellRecord> keyToRecord)
	{
		if (familyIndex >= familyData.Length)
		{
			PolygonCell cell = BuildCellPolygon(intervals, familyData);
			if (cell == null || cell.Vertices.Count < 3)
				return;

			float area = Mathf.Abs(cell.SignedArea());
			if (area <= AreaEpsilon)
				return;

			EnsureClockwise(cell.Vertices);

			GameObject groupClone = UnityEngine.Object.Instantiate(groupPrefab, parent);
			groupClone.name = $"Cell ({string.Join(", ", intervals)})";
			groupClone.transform.localPosition = Vector3.zero;
			groupClone.transform.localRotation = Quaternion.identity;
			groupClone.transform.localScale = Vector3.one;

			Vector3[] verts = new Vector3[cell.Vertices.Count];
			for (int i = 0; i < cell.Vertices.Count; ++i)
			{
				Vector2 p = cell.Vertices[i];
				verts[i] = new Vector3(p.x, p.y, 0f);
			}

			MeshFace face = new MeshFace(modelPrefab, groupClone.transform, verts);

			CellRecord record = new CellRecord
			{
				IntervalIndices = (int[])intervals.Clone(),
				Polygon = cell,
				Face = face
			};

			records.Add(record);
			keyToRecord[BuildIntervalKey(record.IntervalIndices)] = record;
			faces.Add(face);
			return;
		}

		int intervalCount = familyData[familyIndex].Offsets.Length - 1;
		for (int i = 0; i < intervalCount; ++i)
		{
			intervals[familyIndex] = i;
			EnumerateCellsRecursive(
				familyIndex + 1,
				intervals,
				familyData,
				parent,
				groupPrefab,
				modelPrefab,
				faces,
				records,
				keyToRecord);
		}
	}

	private PolygonCell BuildCellPolygon(int[] intervals, FamilyData[] familyData)
	{
		List<Vector2> polygon = BuildBoundaryPolygonCCW();

		for (int i = 0; i < familyData.Length; ++i)
		{
			FamilyData family = familyData[i];
			int interval = intervals[i];

			float lower = family.Offsets[interval];
			float upper = family.Offsets[interval + 1];

			// Keep points with dot(n, p) >= lower
			polygon = ClipPolygonAgainstHalfPlane(polygon, family.Normal, lower, keepGreaterOrEqual: true);
			if (polygon.Count < 3)
				return null;

			// Keep points with dot(n, p) <= upper
			polygon = ClipPolygonAgainstHalfPlane(polygon, family.Normal, upper, keepGreaterOrEqual: false);
			if (polygon.Count < 3)
				return null;
		}

		RemoveNearDuplicateVertices(polygon);

		if (polygon.Count < 3)
			return null;

		PolygonCell cell = new PolygonCell();
		for (int i = 0; i < polygon.Count; ++i)
		{
			cell.Vertices.Add(polygon[i]);
		}

		return cell;
	}

	private List<Vector2> BuildBoundaryPolygonCCW()
	{
		return new List<Vector2>
		{
			new Vector2(boundaryMin.x, boundaryMin.y),
			new Vector2(boundaryMax.x, boundaryMin.y),
			new Vector2(boundaryMax.x, boundaryMax.y),
			new Vector2(boundaryMin.x, boundaryMax.y)
		};
	}

	private List<Vector2> ClipPolygonAgainstHalfPlane(
		List<Vector2> input,
		Vector2 normal,
		float offset,
		bool keepGreaterOrEqual)
	{
		List<Vector2> output = new();
		if (input == null || input.Count == 0)
			return output;

		for (int i = 0; i < input.Count; ++i)
		{
			Vector2 a = input[i];
			Vector2 b = input[(i + 1) % input.Count];

			float da = Vector2.Dot(normal, a) - offset;
			float db = Vector2.Dot(normal, b) - offset;

			bool insideA = keepGreaterOrEqual ? da >= -Epsilon : da <= Epsilon;
			bool insideB = keepGreaterOrEqual ? db >= -Epsilon : db <= Epsilon;

			if (insideA && insideB)
			{
				output.Add(b);
			}
			else if (insideA && !insideB)
			{
				if (TryIntersectSegmentWithLine(a, b, normal, offset, out Vector2 p))
				{
					output.Add(p);
				}
			}
			else if (!insideA && insideB)
			{
				if (TryIntersectSegmentWithLine(a, b, normal, offset, out Vector2 p))
				{
					output.Add(p);
				}
				output.Add(b);
			}
		}

		RemoveNearDuplicateVertices(output);
		return output;
	}

	private bool TryIntersectSegmentWithLine(
		Vector2 a,
		Vector2 b,
		Vector2 normal,
		float offset,
		out Vector2 point)
	{
		point = default;

		Vector2 ab = b - a;
		float denom = Vector2.Dot(normal, ab);

		if (Mathf.Abs(denom) <= Epsilon)
			return false;

		float t = (offset - Vector2.Dot(normal, a)) / denom;
		t = Mathf.Clamp01(t);

		point = a + t * ab;
		return true;
	}

	private void RemoveNearDuplicateVertices(List<Vector2> verts)
	{
		for (int i = verts.Count - 1; i >= 0; --i)
		{
			for (int j = 0; j < i; ++j)
			{
				if ((verts[i] - verts[j]).sqrMagnitude <= Epsilon * Epsilon)
				{
					verts.RemoveAt(i);
					break;
				}
			}
		}

		if (verts.Count >= 2)
		{
			if ((verts[0] - verts[verts.Count - 1]).sqrMagnitude <= Epsilon * Epsilon)
			{
				verts.RemoveAt(verts.Count - 1);
			}
		}
	}

	private void EnsureClockwise(List<Vector2> verts)
	{
		if (SignedArea(verts) > 0f)
		{
			verts.Reverse();
		}
	}

	private float SignedArea(List<Vector2> verts)
	{
		float area = 0f;

		for (int i = 0; i < verts.Count; ++i)
		{
			Vector2 a = verts[i];
			Vector2 b = verts[(i + 1) % verts.Count];
			area += a.x * b.y - b.x * a.y;
		}

		return 0.5f * area;
	}

	private void AssignNeighborsByIntervals(
		List<CellRecord> records,
		Dictionary<string, CellRecord> keyToRecord)
	{
		for (int i = 0; i < records.Count; ++i)
		{
			CellRecord record = records[i];

			for (int axis = 0; axis < record.IntervalIndices.Length; ++axis)
			{
				int[] plus = (int[])record.IntervalIndices.Clone();
				plus[axis] += 1;

				if (keyToRecord.TryGetValue(BuildIntervalKey(plus), out CellRecord plusNeighbor))
				{
					record.Face.AddNeighbor(plusNeighbor.Face);
				}
			}
		}
	}

	private string BuildIntervalKey(int[] intervals)
	{
		return string.Join("_", intervals);
	}

	private void BuildLines(List<Line2D> output)
	{
		output.Clear();

		foreach (LineFamily family in families)
		{
			if (family == null || !family.IsValid())
				continue;

			float angleRad = family.AngleDegrees * Mathf.Deg2Rad;
			Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
			Vector2 normal = new Vector2(-direction.y, direction.x).normalized;

			float currentOffset = family.StartOffset;

			for (int i = 0; i < family.Count; ++i)
			{
				output.Add(new Line2D(normal, currentOffset));
				currentOffset += family.Spacings[i % family.Spacings.Count];
			}
		}
	}

	private void ClipLinesToBoundary(List<Line2D> lines, List<Segment2D> output)
	{
		output.Clear();

		Line2D left = new Line2D(Vector2.right, boundaryMin.x);
		Line2D right = new Line2D(Vector2.right, boundaryMax.x);
		Line2D bottom = new Line2D(Vector2.up, boundaryMin.y);
		Line2D top = new Line2D(Vector2.up, boundaryMax.y);

		for (int i = 0; i < lines.Count; ++i)
		{
			if (TryClipLineToRect(lines[i], left, right, bottom, top, out Segment2D segment))
			{
				output.Add(segment);
			}
		}
	}

	private bool TryClipLineToRect(
		Line2D line,
		Line2D left,
		Line2D right,
		Line2D bottom,
		Line2D top,
		out Segment2D segment)
	{
		List<Vector2> points = new(4);

		TryAddIfInside(line, left, points);
		TryAddIfInside(line, right, points);
		TryAddIfInside(line, bottom, points);
		TryAddIfInside(line, top, points);

		RemoveNearDuplicateVertices(points);

		if (points.Count < 2)
		{
			segment = default;
			return false;
		}

		Vector2 dir = line.Direction.normalized;
		points.Sort((a, b) =>
		{
			float ta = Vector2.Dot(a, dir);
			float tb = Vector2.Dot(b, dir);
			return ta.CompareTo(tb);
		});

		segment = new Segment2D(points[0], points[points.Count - 1]);
		return true;
	}

	private void TryAddIfInside(Line2D a, Line2D b, List<Vector2> points)
	{
		if (!a.TryIntersect(b, out Vector2 p))
			return;

		if (PointInsideRect(p))
			points.Add(p);
	}

	private bool PointInsideRect(Vector2 p, float epsilon = 1e-5f)
	{
		return
			p.x >= boundaryMin.x - epsilon &&
			p.x <= boundaryMax.x + epsilon &&
			p.y >= boundaryMin.y - epsilon &&
			p.y <= boundaryMax.y + epsilon;
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;

		Vector3 bl = new Vector3(boundaryMin.x, boundaryMin.y, 0f);
		Vector3 br = new Vector3(boundaryMax.x, boundaryMin.y, 0f);
		Vector3 tr = new Vector3(boundaryMax.x, boundaryMax.y, 0f);
		Vector3 tl = new Vector3(boundaryMin.x, boundaryMax.y, 0f);

		Gizmos.DrawLine(bl, br);
		Gizmos.DrawLine(br, tr);
		Gizmos.DrawLine(tr, tl);
		Gizmos.DrawLine(tl, bl);

		if (!drawDebugSegments)
			return;

		Gizmos.color = Color.cyan;
		for (int i = 0; i < clippedSegments.Count; ++i)
		{
			Vector2 a = clippedSegments[i].A;
			Vector2 b = clippedSegments[i].B;
			Gizmos.DrawLine(new Vector3(a.x, a.y, 0f), new Vector3(b.x, b.y, 0f));
		}
	}
#endif
}