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
			if (count < 1)
			{
				return false;
			}

			if (spacings == null || spacings.Length == 0)
			{
				return false;
			}

			for (int i = 0; i < spacings.Length; ++i)
			{
				if (spacings[i] <= 0f)
				{
					return false;
				}
			}

			return true;
		}
	}

	[Header("Boundary Rectangle")]
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

	private readonly List<Line2D> builtLines = new List<Line2D>();
	private readonly List<Segment2D> clippedSegments = new List<Segment2D>();

	public IReadOnlyList<Line2D> BuiltLines => builtLines;
	public IReadOnlyList<Segment2D> ClippedSegments => clippedSegments;

	public override void Setup(params int[] numUnitsPerAxis)
	{
		// Not used here.
		// This generator is driven by line families, not grid counts.
	}

	public override bool IsValid()
	{
		if (boundaryMax.x <= boundaryMin.x || boundaryMax.y <= boundaryMin.y)
		{
			return false;
		}

		if (families == null || families.Length == 0)
		{
			return false;
		}

		for (int i = 0; i < families.Length; ++i)
		{
			if (families[i] == null || !families[i].IsValid())
			{
				return false;
			}
		}

		return true;
	}

	public override ICollection<IFace> Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab)
	{
		builtLines.Clear();
		clippedSegments.Clear();

		BuildLines(builtLines);
		ClipLinesToBoundary(builtLines, clippedSegments);

		Debug.Log($"Built {builtLines.Count} lines and {clippedSegments.Count} clipped boundary segments.");

		if (families != null && families.Length == 2)
		{
			return BuildFacesForTwoFamilies(parent, groupPrefab, modelPrefab);
		}

		return BuildFacesFromArrangement(parent, groupPrefab, modelPrefab);
	}

	private ICollection<IFace> BuildFacesForTwoFamilies(
	Transform parent,
	GameObject groupPrefab,
	GameObject modelPrefab)
	{
		List<IFace> faces = new List<IFace>();

		if (families == null || families.Length != 2)
		{
			Debug.LogWarning("BuildFacesForTwoFamilies requires exactly 2 line families.");
			return faces;
		}

		List<Line2D> familyA = BuildLinesForFamily(families[0]);
		List<Line2D> familyB = BuildLinesForFamily(families[1]);

		if (familyA.Count < 2 || familyB.Count < 2)
		{
			Debug.LogWarning("Need at least 2 lines in each family to form cells.");
			return faces;
		}

		MeshFace[,] gridFaces = new MeshFace[familyA.Count - 1, familyB.Count - 1];

		for (int i = 0; i < familyA.Count - 1; ++i)
		{
			for (int j = 0; j < familyB.Count - 1; ++j)
			{
				if (!TryBuildCellCorners(familyA[i], familyA[i + 1], familyB[j], familyB[j + 1],
					out Vector2 p00, out Vector2 p01, out Vector2 p11, out Vector2 p10))
				{
					continue;
				}

				if (!QuadInsideRect(p00, p01, p11, p10))
				{
					continue;
				}

				GameObject groupClone = UnityEngine.Object.Instantiate(groupPrefab, parent);
				groupClone.name = $"Cell ({i}, {j})";
				groupClone.transform.localPosition = Vector3.zero;
				groupClone.transform.localRotation = Quaternion.identity;
				groupClone.transform.localScale = Vector3.one;

				MeshFace face = new MeshFace(
					modelPrefab,
					groupClone.transform,
					new Vector3(p00.x, p00.y, 0f),
					new Vector3(p01.x, p01.y, 0f),
					new Vector3(p11.x, p11.y, 0f),
					new Vector3(p10.x, p10.y, 0f)
				);

				gridFaces[i, j] = face;
				faces.Add(face);
			}
		}

		for (int i = 0; i < familyA.Count - 1; ++i)
		{
			for (int j = 0; j < familyB.Count - 1; ++j)
			{
				MeshFace face = gridFaces[i, j];
				if (face == null)
				{
					continue;
				}

				if (i > 0 && gridFaces[i - 1, j] != null)
				{
					face.AddNeighbor(gridFaces[i - 1, j]);
				}

				if (j > 0 && gridFaces[i, j - 1] != null)
				{
					face.AddNeighbor(gridFaces[i, j - 1]);
				}
			}
		}

		Debug.Log($"Created {faces.Count} faces.");
		return faces;
	}


	private ICollection<IFace> BuildFacesFromArrangement(
	Transform parent,
	GameObject groupPrefab,
	GameObject modelPrefab)
	{
		List<IFace> faces = new List<IFace>();

		if (clippedSegments.Count == 0)
		{
			Debug.LogWarning("No clipped segments available to build faces from.");
			return faces;
		}

		// 1. Split every clipped segment at all valid intersections
		List<Segment2D> splitEdges = BuildSplitEdgesFromArrangement();

		Debug.Log($"Split arrangement into {splitEdges.Count} graph edges.");

		if (splitEdges.Count == 0)
		{
			return faces;
		}

		// 2. Build planar graph
		List<Vector2> graphPoints;
		Dictionary<int, List<int>> adjacency;
		BuildGraph(splitEdges, out graphPoints, out adjacency);

		Debug.Log($"Graph has {graphPoints.Count} vertices.");

		if (graphPoints.Count == 0)
		{
			return faces;
		}

		// 3. Extract polygon loops
		List<PolygonCell> cells = ExtractPolygonCells(graphPoints, adjacency);

		Debug.Log($"Extracted {cells.Count} polygon cells.");

		if (cells.Count == 0)
		{
			return faces;
		}

		// 4. Convert cells into MeshFaces
		List<MeshFace> meshFaces = new List<MeshFace>(cells.Count);

		for (int i = 0; i < cells.Count; ++i)
		{
			PolygonCell cell = cells[i];

			if (cell.Vertices.Count < 3)
			{
				continue;
			}

			cell.EnsureClockwise();

			GameObject groupClone = UnityEngine.Object.Instantiate(groupPrefab, parent);
			groupClone.name = $"Cell {i}";
			groupClone.transform.localPosition = Vector3.zero;
			groupClone.transform.localRotation = Quaternion.identity;
			groupClone.transform.localScale = Vector3.one;

			Vector3[] verts = new Vector3[cell.Vertices.Count];
			for (int v = 0; v < cell.Vertices.Count; ++v)
			{
				Vector2 p = cell.Vertices[v];
				verts[v] = new Vector3(p.x, p.y, 0f);
			}

			MeshFace face = new MeshFace(modelPrefab, groupClone.transform, verts);
			meshFaces.Add(face);
			faces.Add(face);
		}

		// 5. Assign neighbors using shared edges
		AssignNeighbors(meshFaces, cells);

		return faces;
	}
	private void AssignNeighbors(List<MeshFace> meshFaces, List<PolygonCell> cells)
	{
		Dictionary<string, int> edgeOwner = new Dictionary<string, int>();

		for (int i = 0; i < cells.Count; ++i)
		{
			List<Vector2> verts = cells[i].Vertices;

			for (int e = 0; e < verts.Count; ++e)
			{
				Vector2 a = verts[e];
				Vector2 b = verts[(e + 1) % verts.Count];

				string forward = EdgeKey(a, b);
				string reverse = EdgeKey(b, a);

				if (edgeOwner.TryGetValue(reverse, out int otherIndex))
				{
					meshFaces[i].AddNeighbor(meshFaces[otherIndex]);
				}
				else if (!edgeOwner.ContainsKey(forward))
				{
					edgeOwner.Add(forward, i);
				}
			}
		}
	}

	private string EdgeKey(Vector2 a, Vector2 b)
	{
		return $"{Mathf.Round(a.x * 1000f)}_{Mathf.Round(a.y * 1000f)}__{Mathf.Round(b.x * 1000f)}_{Mathf.Round(b.y * 1000f)}";
	}

	private List<Segment2D> BuildSplitEdgesFromArrangement()
	{
		List<Segment2D> result = new List<Segment2D>();

		for (int i = 0; i < clippedSegments.Count; ++i)
		{
			Segment2D baseSegment = clippedSegments[i];
			List<Vector2> pointsOnSegment = new List<Vector2>
		{
			baseSegment.A,
			baseSegment.B
		};

			for (int j = 0; j < builtLines.Count; ++j)
			{
				if (TryIntersectLineWithSegment(builtLines[j], baseSegment, out Vector2 p))
				{
					pointsOnSegment.Add(p);
				}
			}

			RemoveNearDuplicates(pointsOnSegment);

			Vector2 dir = (baseSegment.B - baseSegment.A).normalized;
			pointsOnSegment.Sort((a, b) =>
			{
				float ta = Vector2.Dot(a - baseSegment.A, dir);
				float tb = Vector2.Dot(b - baseSegment.A, dir);
				return ta.CompareTo(tb);
			});

			for (int k = 0; k < pointsOnSegment.Count - 1; ++k)
			{
				Vector2 a = pointsOnSegment[k];
				Vector2 b = pointsOnSegment[k + 1];

				if ((b - a).sqrMagnitude > 1e-8f)
				{
					result.Add(new Segment2D(a, b));
				}
			}
		}

		return result;
	}

	private bool TryIntersectLineWithSegment(
	Line2D line,
	Segment2D segment,
	out Vector2 point,
	float epsilon = 1e-5f)
	{
		point = default;

		// Step 1: Build a Line2D from the segment
		Vector2 segDir = segment.B - segment.A;

		if (segDir.sqrMagnitude < epsilon)
		{
			return false;
		}

		Line2D segmentLine = Line2D.FromPointAndDirection(segment.A, segDir);

		// Step 2: Intersect the two infinite lines
		if (!line.TryIntersect(segmentLine, out point))
		{
			return false; // parallel
		}

		// Step 3: Check if intersection lies ON the segment
		if (!PointOnSegment(point, segment, epsilon))
		{
			return false;
		}

		// Step 4: Check if inside boundary
		if (!PointInsideRect(point, epsilon))
		{
			return false;
		}

		return true;
	}
	private bool PointOnSegment(Vector2 p, Segment2D segment, float epsilon = 1e-5f)
	{
		Vector2 a = segment.A;
		Vector2 b = segment.B;

		Vector2 ab = b - a;
		Vector2 ap = p - a;

		// Check collinearity via cross product
		float cross = ab.x * ap.y - ab.y * ap.x;
		if (Mathf.Abs(cross) > epsilon)
		{
			return false;
		}

		// Check projection lies within segment bounds
		float dot = Vector2.Dot(ap, ab);
		if (dot < -epsilon)
		{
			return false;
		}

		float lenSq = ab.sqrMagnitude;
		if (dot > lenSq + epsilon)
		{
			return false;
		}

		return true;
	}

	private void BuildGraph(
		List<Segment2D> splitEdges,
		out List<Vector2> graphPoints,
		out Dictionary<int, List<int>> adjacency)
	{
		graphPoints = new List<Vector2>();
		adjacency = new Dictionary<int, List<int>>();

		for (int i = 0; i < splitEdges.Count; ++i)
		{
			int a = GetOrAddPoint(graphPoints, splitEdges[i].A);
			int b = GetOrAddPoint(graphPoints, splitEdges[i].B);

			if (a == b)
			{
				continue;
			}

			AddUndirectedEdge(adjacency, a, b);
		}
	}

	private int GetOrAddPoint(List<Vector2> points, Vector2 p, float epsilon = 1e-4f)
	{
		for (int i = 0; i < points.Count; ++i)
		{
			if ((points[i] - p).sqrMagnitude <= epsilon * epsilon)
			{
				return i;
			}
		}

		points.Add(p);
		return points.Count - 1;
	}
	private struct DirectedEdge : IEquatable<DirectedEdge>
	{
		public int From;
		public int To;

		public DirectedEdge(int from, int to)
		{
			From = from;
			To = to;
		}

		public bool Equals(DirectedEdge other)
		{
			return From == other.From && To == other.To;
		}

		public override bool Equals(object obj)
		{
			return obj is DirectedEdge other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (From * 397) ^ To;
			}
		}
	}
	private List<PolygonCell> ExtractPolygonCells(
	List<Vector2> graphPoints,
	Dictionary<int, List<int>> adjacency)
	{
		List<PolygonCell> cells = new List<PolygonCell>();
		HashSet<DirectedEdge> usedDirectedEdges = new HashSet<DirectedEdge>();

		int attemptedStarts = 0;
		int nullLoops = 0;
		int shortLoops = 0;
		int degenerateLoops = 0;
		int wrongWindingLoops = 0;

		foreach (var kvp in adjacency)
		{
			int from = kvp.Key;
			List<int> neighbors = kvp.Value;

			for (int i = 0; i < neighbors.Count; ++i)
			{
				int to = neighbors[i];
				DirectedEdge startEdge = new DirectedEdge(from, to);

				if (usedDirectedEdges.Contains(startEdge))
				{
					continue;
				}

				attemptedStarts++;

				List<DirectedEdge> tracedEdges;
				List<int> loop = TraceFaceLoop(startEdge, graphPoints, adjacency, out tracedEdges);

				if (loop == null)
				{
					nullLoops++;
					Debug.Log($"TraceFaceLoop returned null for start edge {from} -> {to}");
					continue;
				}

				if (loop.Count < 3)
				{
					shortLoops++;
					Debug.Log($"Rejected short loop from {from} -> {to}, count = {loop.Count}");
					continue;
				}

				PolygonCell cell = new PolygonCell();
				for (int v = 0; v < loop.Count; ++v)
				{
					cell.Vertices.Add(graphPoints[loop[v]]);
				}

				float area = cell.SignedArea();
				Debug.Log($"Loop from {from}->{to}, verts={loop.Count}, area={area}");

				if (Mathf.Abs(area) < 1e-4f)
				{
					degenerateLoops++;
					Debug.Log($"Rejected degenerate loop from {from}->{to}");
					continue;
				}

				// Keep only one orientation. Flip this sign if needed.
				if (area < 0f)
				{
					Debug.Log($"Accepted loop from {from}->{to}");

					cells.Add(cell);

					for (int e = 0; e < tracedEdges.Count; ++e)
					{
						usedDirectedEdges.Add(tracedEdges[e]);
					}
				}
				else
				{
					wrongWindingLoops++;
					Debug.Log($"Rejected loop from {from}->{to} due to winding sign");
				}
			}
		}

		Debug.Log(
			$"ExtractPolygonCells summary: attempted={attemptedStarts}, " +
			$"null={nullLoops}, short={shortLoops}, degenerate={degenerateLoops}, " +
			$"wrongWinding={wrongWindingLoops}, acceptedBeforeDedup={cells.Count}");

		RemoveDuplicateCells(cells);

		Debug.Log($"After dedup: {cells.Count} cells");

		return cells;
	}

	private List<int> TraceFaceLoop(
	DirectedEdge startEdge,
	List<Vector2> graphPoints,
	Dictionary<int, List<int>> adjacency,
	out List<DirectedEdge> tracedEdges)
	{
		tracedEdges = new List<DirectedEdge>();
		List<int> loop = new List<int>();

		DirectedEdge current = startEdge;
		int safety = 0;
		const int maxSteps = 2048;

		while (safety++ < maxSteps)
		{
			tracedEdges.Add(current);
			loop.Add(current.From);

			int next = GetNextFaceEdgeTarget(current.From, current.To, graphPoints, adjacency);
			Debug.Log($"Trace step {safety}: {current.From} -> {current.To}, next = {next}");

			if (next < 0)
			{
				Debug.Log($"Trace failed: no next edge from {current.From} -> {current.To}");
				tracedEdges = null;
				return null;
			}

			DirectedEdge nextEdge = new DirectedEdge(current.To, next);

			if (nextEdge.Equals(startEdge))
			{
				Debug.Log($"Closed loop back to start after {safety} steps");
				return loop;
			}

			current = nextEdge;
		}

		Debug.Log("Trace failed: exceeded maxSteps");
		tracedEdges = null;
		return null;
	}

	private int GetNextFaceEdgeTarget(
	int previous,
	int current,
	List<Vector2> graphPoints,
	Dictionary<int, List<int>> adjacency)
	{
		if (!adjacency.TryGetValue(current, out List<int> neighbors) || neighbors.Count == 0)
		{
			Debug.Log($"No neighbors for vertex {current}");
			return -1;
		}

		Vector2 reverseIncoming = (graphPoints[previous] - graphPoints[current]).normalized;
		float baseAngle = Mathf.Atan2(reverseIncoming.y, reverseIncoming.x);

		int bestNeighbor = -1;
		float bestDelta = float.MaxValue;

		for (int i = 0; i < neighbors.Count; ++i)
		{
			int candidate = neighbors[i];

			if (candidate == previous)
			{
				continue;
			}

			Vector2 outgoing = (graphPoints[candidate] - graphPoints[current]).normalized;
			float outgoingAngle = Mathf.Atan2(outgoing.y, outgoing.x);

			float delta = outgoingAngle - baseAngle;
			while (delta <= 0f)
			{
				delta += Mathf.PI * 2f;
			}

			Debug.Log($"At vertex {current}, prev={previous}, candidate={candidate}, delta={delta}");

			if (delta < bestDelta)
			{
				bestDelta = delta;
				bestNeighbor = candidate;
			}
		}

		Debug.Log($"Chosen next from vertex {current}: {bestNeighbor}");
		return bestNeighbor;
	}

	private void RemoveDuplicateCells(List<PolygonCell> cells, float epsilon = 1e-4f)
	{
		for (int i = cells.Count - 1; i >= 0; --i)
		{
			for (int j = 0; j < i; ++j)
			{
				if (CellsEquivalent(cells[i], cells[j], epsilon))
				{
					cells.RemoveAt(i);
					break;
				}
			}
		}
	}

	private bool CellsEquivalent(PolygonCell a, PolygonCell b, float epsilon = 1e-4f)
	{
		if (a.Vertices.Count != b.Vertices.Count)
		{
			return false;
		}

		int n = a.Vertices.Count;

		for (int offset = 0; offset < n; ++offset)
		{
			bool match = true;

			for (int i = 0; i < n; ++i)
			{
				Vector2 av = a.Vertices[i];
				Vector2 bv = b.Vertices[(i + offset) % n];

				if ((av - bv).sqrMagnitude > epsilon * epsilon)
				{
					match = false;
					break;
				}
			}

			if (match)
			{
				return true;
			}
		}

		return false;
	}
	private void AddUndirectedEdge(Dictionary<int, List<int>> adjacency, int a, int b)
	{
		if (!adjacency.TryGetValue(a, out List<int> aList))
		{
			aList = new List<int>();
			adjacency[a] = aList;
		}

		if (!aList.Contains(b))
		{
			aList.Add(b);
		}

		if (!adjacency.TryGetValue(b, out List<int> bList))
		{
			bList = new List<int>();
			adjacency[b] = bList;
		}

		if (!bList.Contains(a))
		{
			bList.Add(a);
		}
	}


	private List<Line2D> BuildLinesForFamily(LineFamily family)
	{
		List<Line2D> result = new List<Line2D>();

		if (family == null || !family.IsValid())
		{
			return result;
		}

		float angleRad = family.AngleDegrees * Mathf.Deg2Rad;
		Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
		Vector2 normal = new Vector2(-direction.y, direction.x).normalized;

		float currentOffset = family.StartOffset;

		for (int i = 0; i < family.Count; ++i)
		{
			result.Add(new Line2D(normal, currentOffset));
			currentOffset += family.Spacings[i % family.Spacings.Count];
		}

		return result;
	}
	private bool TryBuildCellCorners(
	Line2D a0, Line2D a1,
	Line2D b0, Line2D b1,
	out Vector2 p00, out Vector2 p01, out Vector2 p11, out Vector2 p10)
	{
		p00 = default;
		p01 = default;
		p11 = default;
		p10 = default;

		if (!a0.TryIntersect(b0, out p00)) return false;
		if (!a0.TryIntersect(b1, out p01)) return false;
		if (!a1.TryIntersect(b1, out p11)) return false;
		if (!a1.TryIntersect(b0, out p10)) return false;

		return true;
	}

	private bool QuadInsideRect(Vector2 p00, Vector2 p01, Vector2 p11, Vector2 p10, float epsilon = 1e-5f)
	{
		return PointInsideRect(p00, epsilon)
			&& PointInsideRect(p01, epsilon)
			&& PointInsideRect(p11, epsilon)
			&& PointInsideRect(p10, epsilon);
	}

	private bool PointInsideRect(Vector2 p, float epsilon = 1e-5f)
	{
		return
			p.x >= boundaryMin.x - epsilon &&
			p.x <= boundaryMax.x + epsilon &&
			p.y >= boundaryMin.y - epsilon &&
			p.y <= boundaryMax.y + epsilon;
	}

	//public override ICollection<IFace> Generate(Transform parent, GameObject groupPrefab, GameObject modelPrefab)
	//{
	//	builtLines.Clear();
	//	clippedSegments.Clear();

	//	BuildLines(builtLines);
	//	ClipLinesToBoundary(builtLines, clippedSegments);

	//	if (drawDebugSegments)
	//	{
	//		Debug.Log($"Built {builtLines.Count} lines and {clippedSegments.Count} clipped boundary segments.");
	//	}

	//	// Full polygon extraction is not implemented yet.
	//	// Return an empty face list for now so the project still compiles safely.
	//	return new List<IFace>();
	//}

	private void BuildLines(List<Line2D> output)
	{
		output.Clear();

		foreach (LineFamily family in families)
		{
			if (family == null || !family.IsValid())
			{
				continue;
			}

			float angleRad = family.AngleDegrees * Mathf.Deg2Rad;

			Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
			Vector2 normal = new Vector2(-direction.y, direction.x).normalized;

			float currentOffset = family.StartOffset;

			for (int i = 0; i < family.Count; ++i)
			{
				output.Add(new Line2D(normal, currentOffset));

				float spacing = family.Spacings[i % family.Spacings.Count];
				currentOffset += spacing;
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
		List<Vector2> points = new List<Vector2>(4);

		TryAddIfInside(line, left, points);
		TryAddIfInside(line, right, points);
		TryAddIfInside(line, bottom, points);
		TryAddIfInside(line, top, points);

		RemoveNearDuplicates(points);

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
		{
			return;
		}

		if (PointInsideRect(p))
		{
			points.Add(p);
		}
	}

	//private bool PointInsideRect(Vector2 p, float epsilon = 1e-5f)
	//{
	//	return
	//		p.x >= boundaryMin.x - epsilon &&
	//		p.x <= boundaryMax.x + epsilon &&
	//		p.y >= boundaryMin.y - epsilon &&
	//		p.y <= boundaryMax.y + epsilon;
	//}

	private void RemoveNearDuplicates(List<Vector2> points, float epsilon = 1e-4f)
	{
		for (int i = points.Count - 1; i >= 0; --i)
		{
			for (int j = 0; j < i; ++j)
			{
				if ((points[i] - points[j]).sqrMagnitude <= epsilon * epsilon)
				{
					points.RemoveAt(i);
					break;
				}
			}
		}
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