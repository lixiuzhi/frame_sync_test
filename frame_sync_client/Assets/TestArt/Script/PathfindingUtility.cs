using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingUtility
{
	private struct TMNodeInfo
	{
		public TriangleMeshNode node;

		public int vi;

		public Int3 v0;

		public Int3 v1;

		public Int3 v2;

		public IntFactor GetCosineAngle(Int3 dest, out int edgeIndex)
		{
			Int3 vInt = this.v1 - this.v0;
			Int3 vInt2 = this.v2 - this.v0;
			Int3 vInt3 = dest - this.v0;
			vInt3.NormalizeTo(1000);
			vInt.NormalizeTo(1000);
			vInt2.NormalizeTo(1000);
			long num = Int3.DotXZLong(ref vInt3, ref vInt);
			long num2 = Int3.DotXZLong(ref vInt3, ref vInt2);
            IntFactor result = default(IntFactor);
			result.denominator = 1000000L;
			if (num > num2)
			{
				edgeIndex = this.vi;
				result.numerator = num;
			}
			else
			{
				edgeIndex = (this.vi + 2) % 3;
				result.numerator = num2;
			}
			return result;
		}
	}
     
    private static List<TriangleMeshNode> checkedNodes = new List<TriangleMeshNode>();

    public static bool MoveAxisY = true;

	public static int MaxDepth = 4;

	public static int ValidateTargetMaxDepth = 10;

	public static float ValidateTargetRadiusScale = 1.5f;

	private static string acotrName;

	private static Int3[] _staticVerts = new Int3[3];
 

	public static Int3 FindValidTarget(Actor actor, Int3 start, Int3 end, int radius, out bool bResult)
	{
		long num = (long)radius * (long)radius;
		long num2 = start.XZSqrMagnitude(ref end);
		if (num2 < num)
		{
			return PathfindingUtility.FindValidTarget(actor, start, end, out bResult);
		}
		Int3 vInt = end - start;
		Int3 end2 = start + vInt.NormalizeTo(radius);
		return PathfindingUtility.FindValidTarget(actor, start, end2, out bResult);
	}

	public static Int3 FindValidTarget(Actor actor, Int3 start, Int3 end, out bool bResult)
	{
		int actorCamp = (int)actor.CampId;
		TriangleMeshNode triangleMeshNode = null;
		bResult = false;
		if (!AstarPath.active)
		{
			return end;
		}
		AstarData data = AstarPath.active.astarData;
		if (data == null)
		{
			return end;
		}
		int num;
		int num2;
		data.rasterizer.GetCellPosClamped(out num, out num2, start);
		int num3;
		int num4;
		data.rasterizer.GetCellPosClamped(out num3, out num4, end);
		bool flag = num < num3;
		bool flag2 = num2 < num4;
		int num5 = (!flag) ? (num - num3) : (num3 - num);
		int num6 = (!flag2) ? (num2 - num4) : (num4 - num2);
		for (int i = 0; i <= num5; i++)
		{
			for (int j = 0; j <= num6; j++)
			{
				int num7 = num + i * ((!flag) ? -1 : 1);
				int num8 = num2 + j * ((!flag2) ? -1 : 1);
				List<object> objs = data.rasterizer.GetObjs(num7, num8);
				if (objs != null)
				{
					int count = objs.Count;
					if (count != 0)
					{
						Int3 vInt;
						if (count > 2)
						{
							if (data.rasterizer.IntersectionSegment(num7, num8, start, end) && data.CheckSegmentIntersects(start, end, num7, num8, out vInt, out triangleMeshNode))
							{
								if (triangleMeshNode != null)
								{
									Int3 offset = vInt;
									bResult = PathfindingUtility.MakePointInTriangle(ref vInt, triangleMeshNode, -4, 4, -4, 4, offset);
								}
								return vInt;
							}
						}
						else if (data.CheckSegmentIntersects(start, end, num7, num8, out vInt, out triangleMeshNode))
						{
							if (triangleMeshNode != null)
							{
								Int3 offset = vInt;
								bResult = PathfindingUtility.MakePointInTriangle(ref vInt, triangleMeshNode, -4, 4, -4, 4, offset);
							}
							return vInt;
						}
					}
				}
			}
		}
		return end;
	}

	private static TriangleMeshNode getNearestNode(Vector3 position)
	{
		NNConstraint nNConstraint = new NNConstraint();
		nNConstraint.distanceXZ = true;
		nNConstraint.constrainWalkability = false;
		nNConstraint.constrainArea = false;
		nNConstraint.constrainTags = false;
		nNConstraint.constrainDistance = false;
		nNConstraint.graphMask = -1;
		return AstarPath.active.GetNearest(position, nNConstraint).node as TriangleMeshNode;
	}

	public static bool ValidateTarget(Int3 loc, Int3 target, out Int3 newTarget, out int nodeIndex)
	{
		newTarget = target;
		nodeIndex = -1;
		if (!AstarPath.active)
		{
			return false;
		}
		AstarData astarData = AstarPath.active.astarData;
		TriangleMeshNode locatedByRasterizer = astarData.GetLocatedByRasterizer(target);
		if (locatedByRasterizer != null)
		{
			return true;
		}
		int num = -1;
		TriangleMeshNode triangleMeshNode = astarData.IntersectByRasterizer(target, loc, out num);
		if (triangleMeshNode == null)
		{
			return false;
		}
		Int3[] staticVerts = PathfindingUtility._staticVerts;
		triangleMeshNode.GetPoints(out staticVerts[0], out staticVerts[1], out staticVerts[2]);
		bool flag = false;
		Int3 vInt = Polygon.IntersectionPoint(ref target, ref loc, ref staticVerts[num], ref staticVerts[(num + 1) % 3], out flag);
		if (!flag)
		{
			return false;
		}
		if (!PathfindingUtility.MakePointInTriangle(ref vInt, triangleMeshNode, -4, 4, -4, 4, Int3.zero))
		{
			return false;
		}
		newTarget = vInt;
		return true;
	}
     
	public static Int3 Move(Actor actor, Int3 delta, out Int1 groundY)
	{
		//if (actor.isMovable)
		{
            groundY = actor.Position.y; 
            Int3 location = actor.Position;
			return PathfindingUtility.InternalMove(location, delta, ref groundY, actor);
		}
		//groundY = actor.groundY;
		return Int3.zero;
	}

	public static Int3 Move(Actor actor, Int3 delta, out Int1 groundY, out bool collided)
	{
		Int3 result = PathfindingUtility.Move(actor, delta, out groundY);
		collided = (result.x != delta.x || result.z != delta.z);
		return result;
	}

    private static Int3 InternalMove(Int3 srcLoc, Int3 delta, ref Int1 groundY, Actor actor)
    {

        if (!AstarPath.active)
        {
            return Int3.zero;
        } 

        //if (delta.x == 0 && delta.z == 0)
        //{
          //  return delta;
        //} 

        Int3 vInt = srcLoc + delta;
        int startEdge = -1;
        int actorCamp = (int)actor.CampId;
        AstarData data = AstarPath.active.astarData;
        TriangleMeshNode triangleMeshNode = data.GetLocatedByRasterizer(srcLoc);
        if (triangleMeshNode == null)
        {
            TriangleMeshNode triangleMeshNode2 = data.IntersectByRasterizer(srcLoc, vInt, out startEdge);
            if (triangleMeshNode2 == null)
            {
                return Int3.zero;
            }
            triangleMeshNode = triangleMeshNode2; 
        }
        Int3 lhs;

        PathfindingUtility.MoveFromNode(triangleMeshNode, startEdge, srcLoc, vInt , out lhs);

        PathfindingUtility.checkedNodes.Clear();
        groundY = lhs.y; 
        if (!PathfindingUtility.MoveAxisY)
        {
            lhs.y = srcLoc.y;
        }
        return lhs - srcLoc;
    }

	public static bool GetGroundY(Int3 pos, out Int1 groundY)
	{
		if (!AstarPath.active)
		{
			groundY = pos.y;
			return false;
		}
		groundY = pos.y;
		PathfindingUtility.acotrName = "null";
		AstarData astarData = AstarPath.active.astarData;
		TriangleMeshNode locatedByRasterizer = astarData.GetLocatedByRasterizer(pos);
		if (locatedByRasterizer == null)
		{
			return false;
		}
		float f = PathfindingUtility.CalculateY_Clamped((Vector3)pos, locatedByRasterizer);
		groundY = (Int1)f;
		return true;
	}

	public static bool GetGroundY(Actor actor, out Int1 groundY)
	{
		if (!AstarPath.active || actor == null)
		{
			groundY = ((actor == null) ? 0 :(int)actor.ActorTransform.position.y);
			return false;
		}
		groundY = (int)actor.ActorTransform.position.y;
		PathfindingUtility.acotrName = ((actor == null) ? string.Empty : actor.ActorId.ToString());
		Int3 location = (Int3)actor.ActorTransform.position;
		AstarData astarData = AstarPath.active.astarData;
		TriangleMeshNode locatedByRasterizer = astarData.GetLocatedByRasterizer(location);
		if (locatedByRasterizer == null)
		{
			return false;
		}
		float f = PathfindingUtility.CalculateY_Clamped((Vector3)location, locatedByRasterizer);
		groundY = (Int1)f;
		return true;
	}

	private static void GetAllNodesByVert(ref List<PathfindingUtility.TMNodeInfo> nodeInfos, TriangleMeshNode startNode, int vertIndex)
	{
		if (nodeInfos == null)
		{
			nodeInfos = new List<PathfindingUtility.TMNodeInfo>();
		}
		for (int i = 0; i < nodeInfos.Count; i++)
		{
			if (nodeInfos[i].node == startNode)
			{
				return;
			}
		}
		int num;
		if (startNode.v0 == vertIndex)
		{
			num = 0;
		}
		else if (startNode.v1 == vertIndex)
		{
			num = 1;
		}
		else
		{
			if (startNode.v2 != vertIndex)
			{
				return;
			}
			num = 2;
		}
		PathfindingUtility.TMNodeInfo tMNodeInfo = default(PathfindingUtility.TMNodeInfo);
		tMNodeInfo.vi = num;
		tMNodeInfo.node = startNode;
		tMNodeInfo.v0 = startNode.GetVertex(num % 3);
		tMNodeInfo.v1 = startNode.GetVertex((num + 1) % 3);
		tMNodeInfo.v2 = startNode.GetVertex((num + 2) % 3);
		nodeInfos.Add(tMNodeInfo);
		if (startNode.connections != null)
		{
			for (int j = 0; j < startNode.connections.Length; j++)
			{
				TriangleMeshNode triangleMeshNode = startNode.connections[j] as TriangleMeshNode;
				if (triangleMeshNode != null && triangleMeshNode.GraphIndex == startNode.GraphIndex)
				{
					PathfindingUtility.GetAllNodesByVert(ref nodeInfos, triangleMeshNode, vertIndex);
				}
			}
		}
	}

	private static bool MakePointInTriangle(ref Int3 result, TriangleMeshNode node, int minX, int maxX, int minZ, int maxZ, Int3 offset)
	{
		Int3 vInt;
		Int3 vInt2;
		Int3 vInt3;
		node.GetPoints(out vInt, out vInt2, out vInt3);
		long num = (long)(vInt2.x - vInt.x);
		long num2 = (long)(vInt3.x - vInt2.x);
		long num3 = (long)(vInt.x - vInt3.x);
		long num4 = (long)(vInt2.z - vInt.z);
		long num5 = (long)(vInt3.z - vInt2.z);
		long num6 = (long)(vInt.z - vInt3.z);
		for (int i = minX; i <= maxX; i++)
		{
			for (int j = minZ; j <= maxZ; j++)
			{
				int num7 = i + offset.x;
				int num8 = j + offset.z;
				if (num * (long)(num8 - vInt.z) - (long)(num7 - vInt.x) * num4 <= 0L && num2 * (long)(num8 - vInt2.z) - (long)(num7 - vInt2.x) * num5 <= 0L && num3 * (long)(num8 - vInt3.z) - (long)(num7 - vInt3.x) * num6 <= 0L)
				{
					result.x = num7;
					result.z = num8;
					return true;
				}
			}
		}
		return false;
	}

	private static void getMinMax(out int min, out int max, long axis, ref IntFactor factor)
	{
		long num = axis * factor.numerator;
		int num2 = (int)(num / factor.denominator);
		if (num < 0L)
		{
			min = num2 - 1;
			max = num2;
		}
		else
		{
			min = num2;
			max = num2 + 1;
		}
	}

	private static void MoveAlongEdge(TriangleMeshNode node, int edge, Int3 srcLoc, Int3 destLoc , out Int3 result, bool checkAnotherEdge = true)
	{ 
		Int3 vertex = node.GetVertex(edge);
		Int3 vertex2 = node.GetVertex((edge + 1) % 3);
		Int3 vInt = destLoc - srcLoc;
		vInt.y = 0;
		Int3 vInt2 = vertex2 - vertex;
		vInt2.y = 0;
		vInt2.NormalizeTo(1000);
		int num;

        num = vInt2.x * vInt.x + vInt2.z * vInt.z; 

		bool flag;
		Int3 rhs = Polygon.IntersectionPoint(ref vertex, ref vertex2, ref srcLoc, ref destLoc, out flag);
		if (!flag)
		{
			if (!Polygon.IsColinear(vertex, vertex2, srcLoc) || !Polygon.IsColinear(vertex, vertex2, destLoc))
			{
				result = srcLoc;
				return;
			}
			if (num >= 0)
			{
				int num2 = vInt2.x * (vertex2.x - vertex.x) + vInt2.z * (vertex2.z - vertex.z);
				int num3 = vInt2.x * (destLoc.x - vertex.x) + vInt2.z * (destLoc.z - vertex.z);
				rhs = ((num2 <= num3) ? vertex2 : destLoc); 
			}
			else
			{
				int num4 = -vInt2.x * (vertex.x - vertex2.x) - vInt2.z * (vertex.z - vertex2.z);
				int num5 = -vInt2.x * (destLoc.x - vertex2.x) - vInt2.z * (destLoc.z - vertex2.z);
				rhs = ((Mathf.Abs(num4) <= Mathf.Abs(num5)) ? vertex : destLoc); 
			}
		}
		int num6 = -IntMath.Sqrt(vertex.XZSqrMagnitude(rhs) * 1000000L);
		int num7 = IntMath.Sqrt(vertex2.XZSqrMagnitude(rhs) * 1000000L);
		if (num >= num6 && num <= num7)
		{
			result = IntMath.Divide(vInt2, (long)num, 1000000L) + rhs;
			if (!node.ContainsPoint(result))
			{
				Vector3 vector = (Vector3)(vertex2 - vertex);
				vector.y = 0f;
				vector.Normalize();
				Int3 lhs = vertex2 - vertex;
				lhs.y = 0;
				lhs *= 10000;
				long num8 = (long)lhs.magnitude;
				IntFactor vFactor = default(IntFactor);
				vFactor.numerator = (long)num;
				vFactor.denominator = num8 * 1000L;
				int num9;
				int num10;
				PathfindingUtility.getMinMax(out num9, out num10, (long)lhs.x, ref vFactor);
				int num11;
				int num12;
				PathfindingUtility.getMinMax(out num11, out num12, (long)lhs.z, ref vFactor);
				if (!PathfindingUtility.MakePointInTriangle(ref result, node, num9, num10, num11, num12, srcLoc) && !PathfindingUtility.MakePointInTriangle(ref result, node, num9 - 4, num10 + 4, num11 - 4, num12 + 4, srcLoc))
				{
					result = srcLoc;
				}
			}
			if (PathfindingUtility.MoveAxisY)
			{
				PathfindingUtility.CalculateY(ref result, node);
			}
		}
		else
		{
			int rhs2;
			int edge2;
			Int3 vInt4;
			if (num < num6)
			{
				rhs2 = num - num6;
				edge2 = (edge + 2) % 3;
				vInt4 = vertex;
			}
			else
			{
				rhs2 = num - num7;
				edge2 = (edge + 1) % 3;
				vInt4 = vertex2;
			}
			Int3 vInt5 = vInt2 * rhs2 / 1000000f;
			int startEdge;
			TriangleMeshNode neighborByEdge = node.GetNeighborByEdge(edge2, out startEdge);
			if (neighborByEdge != null)
			{
				PathfindingUtility.checkedNodes.Add(node);
				PathfindingUtility.MoveFromNode(neighborByEdge, startEdge, vInt4, vInt5 + vInt4, out result);
			}
			else
			{
				if (checkAnotherEdge)
				{
					Int3 vertex3 = node.GetVertex((edge + 2) % 3);
					Int3 lhs2 = (vertex3 - vInt4).NormalizeTo(1000);
					if (Int3.Dot(lhs2, vInt5) > 0)
					{
						PathfindingUtility.checkedNodes.Add(node);
						PathfindingUtility.MoveAlongEdge(node, edge2, vInt4, vInt5 + vInt4, out result, false);
						return;
					}
				}
				result = vInt4;
			}
		}
	}

	private static void MoveFromNode(TriangleMeshNode node, int startEdge, Int3 srcLoc, Int3 destLoc , out Int3 result)
	{
		result = srcLoc;
		while (node != null)
		{
			int count = 2;
			int i;
			if (node.IsVertex(srcLoc, out i))
			{
				int vertexIndex = node.GetVertexIndex(i);
				List<PathfindingUtility.TMNodeInfo> list = null;
				PathfindingUtility.GetAllNodesByVert(ref list, node, vertexIndex);
				TriangleMeshNode triangleMeshNode = null;
				int num = -1;
				for (int j = 0; j < list.Count; j++)
				{
					PathfindingUtility.TMNodeInfo tMNodeInfo = list[j];
					if (!PathfindingUtility.checkedNodes.Contains(tMNodeInfo.node) && !Polygon.LeftNotColinear(tMNodeInfo.v0, tMNodeInfo.v2, destLoc) && Polygon.Left(tMNodeInfo.v0, tMNodeInfo.v1, destLoc))
					{
						triangleMeshNode = tMNodeInfo.node;
						num = tMNodeInfo.vi;
						break;
					}
				}
				if (triangleMeshNode != null)
				{
					node = triangleMeshNode;
					startEdge = (num + 1) % 3;
					count = 1;
				}
				else
				{
					int edge = -1;
					IntFactor b = new IntFactor
					{
						numerator = -2L,
						denominator = 1L
					};
					for (int k = 0; k < list.Count; k++)
					{
						PathfindingUtility.TMNodeInfo tMNodeInfo2 = list[k];
						if (!PathfindingUtility.checkedNodes.Contains(tMNodeInfo2.node))
						{
							int num2;
							IntFactor cosineAngle = tMNodeInfo2.GetCosineAngle(destLoc, out num2);
							if (cosineAngle > b)
							{
								b = cosineAngle;
								edge = num2;
								triangleMeshNode = tMNodeInfo2.node;
							}
						}
					}
					if (triangleMeshNode != null)
					{
						PathfindingUtility.MoveAlongEdge(triangleMeshNode, edge, srcLoc, destLoc, out result, true);
						break;
					}
				}
			}
			int num3;
			if (startEdge == -1)
			{
				num3 = node.EdgeIntersect(srcLoc, destLoc);
			}
			else
			{
				num3 = node.EdgeIntersect(srcLoc, destLoc, startEdge, count);
			}
			if (num3 == -1)
			{
				if (node.ContainsPoint(destLoc))
				{
					result = destLoc;
					if (PathfindingUtility.MoveAxisY)
					{
						PathfindingUtility.CalculateY(ref result, node);
					}
				}
				else
				{
					num3 = node.GetColinearEdge(srcLoc, destLoc);
					if (num3 != -1)
					{
						PathfindingUtility.MoveAlongEdge(node, num3, srcLoc, destLoc,  out result, true);
					}
				}
				break;
			}
			int num4;
			TriangleMeshNode neighborByEdge = node.GetNeighborByEdge(num3, out num4);
			if (neighborByEdge == null)
			{
				PathfindingUtility.MoveAlongEdge(node, num3, srcLoc, destLoc,  out result, true);
				break;
			}
			node = neighborByEdge;
			startEdge = num4 + 1;
		}
	}

	private static void CalculateY(ref Int3 point, TriangleMeshNode node)
	{
		float num = PathfindingUtility.CalculateY((Vector3)point, node);
		point.y = Mathf.RoundToInt(num * 1000f);
	}

	private static float CalculateY(Vector3 pf, TriangleMeshNode node)
	{
		Vector3 vector;
		Vector3 vector2;
		Vector3 vector3;
		node.GetPoints(out vector, out vector2, out vector3);
		float num = (vector2.z - vector3.z) * (vector.x - vector3.x) + (vector3.x - vector2.x) * (vector.z - vector3.z);
		float num2 = 1f / num;
		float num3 = (vector2.z - vector3.z) * (pf.x - vector3.x) + (vector3.x - vector2.x) * (pf.z - vector3.z);
		num3 *= num2;
		float num4 = (vector3.z - vector.z) * (pf.x - vector3.x) + (vector.x - vector3.x) * (pf.z - vector3.z);
		num4 *= num2;
		float num5 = 1f - num3 - num4;
		return num3 * vector.y + num4 * vector2.y + num5 * vector3.y;
	}

	private static float CalculateY_Clamped(Vector3 pf, TriangleMeshNode node)
	{
		Vector3 vector;
		Vector3 vector2;
		Vector3 vector3;
		node.GetPoints(out vector, out vector2, out vector3);
		float num = (vector2.z - vector3.z) * (vector.x - vector3.x) + (vector3.x - vector2.x) * (vector.z - vector3.z);
		float num2 = 1f / num;
		float num3 = (vector2.z - vector3.z) * (pf.x - vector3.x) + (vector3.x - vector2.x) * (pf.z - vector3.z);
		num3 *= num2;
		num3 = Mathf.Clamp01(num3);
		float num4 = (vector3.z - vector.z) * (pf.x - vector3.x) + (vector.x - vector3.x) * (pf.z - vector3.z);
		num4 *= num2;
		num4 = Mathf.Clamp01(num4);
		float num5 = Mathf.Clamp01(1f - num3 - num4);
		return num3 * vector.y + num4 * vector2.y + num5 * vector3.y;
	}
}
