using UnityEngine;
using Pathfinding.Serialization;

namespace Pathfinding {
	public interface INavmeshHolder {
		Int3 GetVertex (int i);
		int GetVertexArrayIndex (int index);
		void GetTileCoordinates (int tileIndex, out int x, out int z);
	}

	/** Node represented by a triangle */
	public class TriangleMeshNode : MeshNode {
		public TriangleMeshNode (AstarPath astar) : base(astar) {}

		/** Internal vertex index for the first vertex */
		public int v0;

		/** Internal vertex index for the second vertex */
		public int v1;

		/** Internal vertex index for the third vertex */
		public int v2; 

        private static Int3[] _staticVerts = new Int3[3];

        /** Holds INavmeshHolder references for all graph indices to be able to access them in a performant manner */
        protected static INavmeshHolder[] _navmeshHolders = new INavmeshHolder[0];

		/** Used for synchronised access to the #_navmeshHolders array */
		protected static readonly System.Object lockObject = new System.Object();

        public void GetPoints(out Int3 a, out Int3 b, out Int3 c)
        {
            INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(GraphIndex);
            a = navmeshHolder.GetVertex(this.v0);
            b = navmeshHolder.GetVertex(this.v1);
            c = navmeshHolder.GetVertex(this.v2);
        }
        public void GetPoints(out Vector3 a, out Vector3 b, out Vector3 c)
        {
            INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder( base.GraphIndex);
            a = (Vector3)navmeshHolder.GetVertex(this.v0);
            b = (Vector3)navmeshHolder.GetVertex(this.v1);
            c = (Vector3)navmeshHolder.GetVertex(this.v2);
        }
        public bool IsVertex(Int3 p, out int index)
        {
            INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(base.GraphIndex);
            index = -1;
            if (navmeshHolder.GetVertex(this.v0).IsEqualXZ(ref p))
            {
                index = 0;
            }
            else if (navmeshHolder.GetVertex(this.v1).IsEqualXZ(ref p))
            {
                index = 1;
            }
            else if (navmeshHolder.GetVertex(this.v2).IsEqualXZ(ref p))
            {
                index = 2;
            }
            return index != -1;
        }
         
        public int EdgeIntersect(Int3 a, Int3 b)
        {
            Int3 vInt;
            Int3 vInt2;
            Int3 vInt3;
            this.GetPoints(out vInt, out vInt2, out vInt3);
            if (Polygon.Intersects(vInt, vInt2, a, b))
            {
                return 0;
            }
            if (Polygon.Intersects(vInt2, vInt3, a, b))
            {
                return 1;
            }
            if (Polygon.Intersects(vInt3, vInt, a, b))
            {
                return 2;
            }
            return -1;
        }


        public int EdgeIntersect(Int3 a, Int3 b, int startEdge, int count)
        {
            Int3[] staticVerts = TriangleMeshNode._staticVerts;
            this.GetPoints(out staticVerts[0], out staticVerts[1], out staticVerts[2]);
            for (int i = 0; i < count; i++)
            {
                int num = (startEdge + i) % 3;
                int num2 = (num + 1) % 3;
                if (Polygon.Intersects(staticVerts[num], staticVerts[num2], a, b))
                {
                    return num;
                }
            }
            return -1;
        }


        public static INavmeshHolder GetNavmeshHolder (uint graphIndex) {
			return _navmeshHolders[(int)graphIndex];
		}

		/** Sets the internal navmesh holder for a given graph index.
		 * \warning Internal method
		 */
		public static void SetNavmeshHolder (int graphIndex, INavmeshHolder graph) {
			if (_navmeshHolders.Length <= graphIndex) {
				// We need to lock and then check again to make sure
				// that this the resize operation is thread safe
				lock (lockObject) {
					if (_navmeshHolders.Length <= graphIndex) {
						var gg = new INavmeshHolder[graphIndex+1];
						for (int i = 0; i < _navmeshHolders.Length; i++) gg[i] = _navmeshHolders[i];
						_navmeshHolders = gg;
					}
				}
			}
			_navmeshHolders[graphIndex] = graph;
		}

		/** Set the position of this node to the average of its 3 vertices */
		public void UpdatePositionFromVertices () {
			INavmeshHolder g = GetNavmeshHolder(GraphIndex);

			position = (g.GetVertex(v0) + g.GetVertex(v1) + g.GetVertex(v2)) * 0.333333f;
		}

		/** Return a number identifying a vertex.
		 * This number does not necessarily need to be a index in an array but two different vertices (in the same graph) should
		 * not have the same vertex numbers.
		 */
		public int GetVertexIndex (int i) {
			return i == 0 ? v0 : (i == 1 ? v1 : v2);
		}

		/** Return a number specifying an index in the source vertex array.
		 * The vertex array can for example be contained in a recast tile, or be a navmesh graph, that is graph dependant.
		 * This is slower than GetVertexIndex, if you only need to compare vertices, use GetVertexIndex.
		 */
		public int GetVertexArrayIndex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertexArrayIndex(i == 0 ? v0 : (i == 1 ? v1 : v2));
		}

		public override Int3 GetVertex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertex(GetVertexIndex(i));
		}

		public override int GetVertexCount () {
			// A triangle has 3 vertices
			return 3;
		}

		public override Vector3 ClosestPointOnNode (Vector3 p) {
			INavmeshHolder g = GetNavmeshHolder(GraphIndex);

			return Pathfinding.Polygon.ClosestPointOnTriangle((Vector3)g.GetVertex(v0), (Vector3)g.GetVertex(v1), (Vector3)g.GetVertex(v2), p);
		}

		public override Vector3 ClosestPointOnNodeXZ (Vector3 p) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			INavmeshHolder g = GetNavmeshHolder(GraphIndex);

			// Get all 3 vertices for this node
			Int3 tp1 = g.GetVertex(v0);
			Int3 tp2 = g.GetVertex(v1);
			Int3 tp3 = g.GetVertex(v2);

			Vector2 closest = Polygon.ClosestPointOnTriangle(
				new Vector2(tp1.x*Int3.PrecisionFactor, tp1.z*Int3.PrecisionFactor),
				new Vector2(tp2.x*Int3.PrecisionFactor, tp2.z*Int3.PrecisionFactor),
				new Vector2(tp3.x*Int3.PrecisionFactor, tp3.z*Int3.PrecisionFactor),
				new Vector2(p.x, p.z)
				);

			return new Vector3(closest.x, p.y, closest.y);
		}

        public int GetColinearEdge(Int3 a, Int3 b)
        {
            Int3 vInt;
            Int3 vInt2;
            Int3 vInt3;
            this.GetPoints(out vInt, out vInt2, out vInt3);
            if (Polygon.IsColinear(vInt, vInt2, a) && Polygon.IsColinear(vInt, vInt2, b))
            {
                return 0;
            }
            if (Polygon.IsColinear(vInt2, vInt3, a) && Polygon.IsColinear(vInt2, vInt3, b))
            {
                return 1;
            }
            if (Polygon.IsColinear(vInt3, vInt, a) && Polygon.IsColinear(vInt3, vInt, b))
            {
                return 2;
            }
            return -1;
        }

        public int GetColinearEdge(Int3 a, Int3 b, int startEdge, int count)
        {
            Int3[] staticVerts = TriangleMeshNode._staticVerts;
            this.GetPoints(out staticVerts[0], out staticVerts[1], out staticVerts[2]);
            for (int i = 0; i < count; i++)
            {
                int num = (startEdge + i) % 3;
                int num2 = (num + 1) % 3;
                if (Polygon.IsColinear(staticVerts[num], staticVerts[num2], a) && Polygon.IsColinear(staticVerts[num], staticVerts[num2], b))
                {
                    return num;
                }
            }
            return -1;
        }

        public TriangleMeshNode GetNeighborByEdge(int edge, out int otherEdge)
        {
            otherEdge = -1;
            if (edge < 0 || edge > 2 || this.connections == null)
            {
                return null;
            }
            int vertexIndex = this.GetVertexIndex(edge % 3);
            int vertexIndex2 = this.GetVertexIndex((edge + 1) % 3);
            TriangleMeshNode result = null;
            for (int i = 0; i < this.connections.Length; i++)
            {
                TriangleMeshNode triangleMeshNode = this.connections[i] as TriangleMeshNode;
                if (triangleMeshNode != null && triangleMeshNode.GraphIndex == base.GraphIndex)
                {
                    if (triangleMeshNode.v1 == vertexIndex && triangleMeshNode.v0 == vertexIndex2)
                    {
                        otherEdge = 0;
                    }
                    else if (triangleMeshNode.v2 == vertexIndex && triangleMeshNode.v1 == vertexIndex2)
                    {
                        otherEdge = 1;
                    }
                    else if (triangleMeshNode.v0 == vertexIndex && triangleMeshNode.v2 == vertexIndex2)
                    {
                        otherEdge = 2;
                    }
                    if (otherEdge != -1)
                    {
                        result = triangleMeshNode;
                        break;
                    }
                }
            }
            return result;
        }

        public Int3 ClosestPointOnNodeXZ(Int3 p)
        {
            INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(base.GraphIndex);
            Int3 vertex = navmeshHolder.GetVertex(this.v0);
            Int3 vertex2 = navmeshHolder.GetVertex(this.v1);
            Int3 vertex3 = navmeshHolder.GetVertex(this.v2);
            vertex.y = 0;
            vertex2.y = 0;
            vertex3.y = 0;
            Int3 result;
            if ((long)(vertex2.x - vertex.x) * (long)(p.z - vertex.z) - (long)(p.x - vertex.x) * (long)(vertex2.z - vertex.z) > 0L)
            {
                this.CalcNearestPoint(out result, ref vertex, ref vertex2, ref p);
            }
            else if ((long)(vertex3.x - vertex2.x) * (long)(p.z - vertex2.z) - (long)(p.x - vertex2.x) * (long)(vertex3.z - vertex2.z) > 0L)
            {
                this.CalcNearestPoint(out result, ref vertex2, ref vertex3, ref p);
            }
            else if ((long)(vertex.x - vertex3.x) * (long)(p.z - vertex3.z) - (long)(p.x - vertex3.x) * (long)(vertex.z - vertex3.z) > 0L)
            {
                this.CalcNearestPoint(out result, ref vertex3, ref vertex, ref p);
            }
            else
            {
                result = p;
            }
            return result;
        }

        private void CalcNearestPoint(out Int3 cp, ref Int3 start, ref Int3 end, ref Int3 p)
        {
            Int2 vInt = new Int2(end.x - start.x, end.z - start.z);
            long sqrMagnitudeLong = vInt.sqrMagnitudeLong;
            Int2 vInt2 = new Int2(p.x - start.x, p.z - start.z);
            cp = default(Int3);
            cp.y = p.y;
            long num = Int2.DotLong(ref vInt2, ref vInt);
            if (sqrMagnitudeLong != 0L)
            {
                long a = (long)(end.x - start.x) * num;
                long a2 = (long)(end.z - start.z) * num;
                cp.x = (int)IntMath.Divide(a, sqrMagnitudeLong);
                cp.z = (int)IntMath.Divide(a2, sqrMagnitudeLong);
                cp.x += start.x;
                cp.z += start.z;
            }
            else
            {
                int num2 = (int)num;
                cp.x = start.x + (end.x - start.x) * num2;
                cp.z = start.z + (end.z - start.z) * num2;
            }
        }

        public override bool ContainsPoint (Int3 p) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			INavmeshHolder navmeshHolder = GetNavmeshHolder(GraphIndex);

			// Get all 3 vertices for this node
			Int3 a = navmeshHolder.GetVertex(v0);
			Int3 b = navmeshHolder.GetVertex(v1);
			Int3 c = navmeshHolder.GetVertex(v2);

			if ((long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) > 0) return false;

			if ((long)(c.x - b.x) * (long)(p.z - b.z) - (long)(p.x - b.x) * (long)(c.z - b.z) > 0) return false;

			if ((long)(a.x - c.x) * (long)(p.z - c.z) - (long)(p.x - c.x) * (long)(a.z - c.z) > 0) return false;

			return true;
			// Equivalent code, but the above code is faster
			//return Polygon.IsClockwiseMargin (a,b, p) && Polygon.IsClockwiseMargin (b,c, p) && Polygon.IsClockwiseMargin (c,a, p);

			//return Polygon.ContainsPoint(g.GetVertex(v0),g.GetVertex(v1),g.GetVertex(v2),p);
		}

		public override void UpdateRecursiveG (Path path, PathNode pathNode, PathHandler handler) {
			UpdateG(path, pathNode);

			handler.heap.Add(pathNode);

			if (connections == null) return;

			for (int i = 0; i < connections.Length; i++) {
				GraphNode other = connections[i];
				PathNode otherPN = handler.GetPathNode(other);
				if (otherPN.parent == pathNode && otherPN.pathID == handler.PathID) other.UpdateRecursiveG(path, otherPN, handler);
			}
		}

		public override void Open (Path path, PathNode pathNode, PathHandler handler) {
			if (connections == null) return;

			// Flag2 indicates if this node needs special treatment
			// with regard to connection costs
			bool flag2 = pathNode.flag2;

			// Loop through all connections
			for (int i = connections.Length-1; i >= 0; i--) {
				GraphNode other = connections[i];

				// Make sure we can traverse the neighbour
				if (path.CanTraverse(other)) {
					PathNode pathOther = handler.GetPathNode(other);

					// Fast path out, worth it for triangle mesh nodes since they usually have degree 2 or 3
					if (pathOther == pathNode.parent) {
						continue;
					}

					uint cost = connectionCosts[i];

					if (flag2 || pathOther.flag2) {
						// Get special connection cost from the path
						// This is used by the start and end nodes
						cost = path.GetConnectionSpecialCost(this, other, cost);
					}

					// Test if we have seen the other node before
					if (pathOther.pathID != handler.PathID) {
						// We have not seen the other node before
						// So the path from the start through this node to the other node
						// must be the shortest one so far

						// Might not be assigned
						pathOther.node = other;

						pathOther.parent = pathNode;
						pathOther.pathID = handler.PathID;

						pathOther.cost = cost;

						pathOther.H = path.CalculateHScore(other);
						other.UpdateG(path, pathOther);

						handler.heap.Add(pathOther);
					} else {
						// If not we can test if the path from this node to the other one is a better one than the one already used
						if (pathNode.G + cost + path.GetTraversalCost(other) < pathOther.G) {
							pathOther.cost = cost;
							pathOther.parent = pathNode;

							other.UpdateRecursiveG(path, pathOther, handler);
						} else if (pathOther.G+cost+path.GetTraversalCost(this) < pathNode.G && other.ContainsConnection(this)) {
							// Or if the path from the other node to this one is better

							pathNode.parent = pathOther;
							pathNode.cost = cost;

							UpdateRecursiveG(path, pathNode, handler);
						}
					}
				}
			}
		}

		/** Returns the edge which is shared with \a other.
		 * If no edge is shared, -1 is returned.
		 * The edge is GetVertex(result) - GetVertex((result+1) % GetVertexCount()).
		 * See GetPortal for the exact segment shared.
		 * \note Might return that an edge is shared when the two nodes are in different tiles and adjacent on the XZ plane, but do not line up perfectly on the Y-axis.
		 * Therefore it is recommended that you only test for neighbours of this node or do additional checking afterwards.
		 */
		public int SharedEdge (GraphNode other) {
			int a, b;

			GetPortal(other, null, null, false, out a, out b);
			return a;
		}

		public override bool GetPortal (GraphNode _other, System.Collections.Generic.List<Int3> left, System.Collections.Generic.List<Int3> right, bool backwards) {
			int aIndex, bIndex;

			return GetPortal(_other, left, right, backwards, out aIndex, out bIndex);
		}

		public bool GetPortal (GraphNode _other, System.Collections.Generic.List<Int3> left, System.Collections.Generic.List<Int3> right, bool backwards, out int aIndex, out int bIndex) {
			aIndex = -1;
			bIndex = -1;

			//If the nodes are in different graphs, this function has no idea on how to find a shared edge.
			if (_other.GraphIndex != GraphIndex) return false;

			// Since the nodes are in the same graph, they are both TriangleMeshNodes
			// So we don't need to care about other types of nodes
			var other = _other as TriangleMeshNode;

			//Get tile indices
			int tileIndex = (GetVertexIndex(0) >> RecastGraph.TileIndexOffset) & RecastGraph.TileIndexMask;
			int tileIndex2 = (other.GetVertexIndex(0) >> RecastGraph.TileIndexOffset) & RecastGraph.TileIndexMask;

			//When the nodes are in different tiles, the edges might not be completely identical
			//so another technique is needed
			//Only do this on recast graphs
			if (tileIndex != tileIndex2 && (GetNavmeshHolder(GraphIndex) is RecastGraph)) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].GraphIndex != GraphIndex) {
//#if !ASTAR_NO_POINT_GRAPH
//						var mid = connections[i] as NodeLink3Node;
//						if (mid != null && mid.GetOther(this) == other) {
//							// We have found a node which is connected through a NodeLink3Node

//							if (left != null) {
//								mid.GetPortal(other, left, right, false);
//								return true;
//							}
//						}
//#endif
					}
				}

				//Get the tile coordinates, from them we can figure out which edge is going to be shared
				int x1, x2, z1, z2;
				int coord;
				INavmeshHolder nm = GetNavmeshHolder(GraphIndex);
				nm.GetTileCoordinates(tileIndex, out x1, out z1);
				nm.GetTileCoordinates(tileIndex2, out x2, out z2);

				if (System.Math.Abs(x1-x2) == 1) coord = 0;
				else if (System.Math.Abs(z1-z2) == 1) coord = 2;
				else throw new System.Exception("Tiles not adjacent (" + x1+", " + z1 +") (" + x2 + ", " + z2+")");

				int av = GetVertexCount();
				int bv = other.GetVertexCount();

				//Try the X and Z coordinate. For one of them the coordinates should be equal for one of the two nodes' edges
				//The midpoint between the tiles is the only place where they will be equal

				int first = -1, second = -1;

				//Find the shared edge
				for (int a = 0; a < av; a++) {
					int va = GetVertex(a)[coord];
					for (int b = 0; b < bv; b++) {
						if (va == other.GetVertex((b+1)%bv)[coord] && GetVertex((a+1) % av)[coord] == other.GetVertex(b)[coord]) {
							first = a;
							second = b;
							a = av;
							break;
						}
					}
				}

				aIndex = first;
				bIndex = second;

				if (first != -1) {
					Int3 a = GetVertex(first);
					Int3 b = GetVertex((first+1)%av);

					//The coordinate which is not the same for the vertices
					int ocoord = coord == 2 ? 0 : 2;

					//When the nodes are in different tiles, they might not share exactly the same edge
					//so we clamp the portal to the segment of the edges which they both have.
					int mincoord = System.Math.Min(a[ocoord], b[ocoord]);
					int maxcoord = System.Math.Max(a[ocoord], b[ocoord]);

					mincoord = System.Math.Max(mincoord, System.Math.Min(other.GetVertex(second)[ocoord], other.GetVertex((second+1)%bv)[ocoord]));
					maxcoord = System.Math.Min(maxcoord, System.Math.Max(other.GetVertex(second)[ocoord], other.GetVertex((second+1)%bv)[ocoord]));

					if (a[ocoord] < b[ocoord]) {
						a[ocoord] = mincoord;
						b[ocoord] = maxcoord;
					} else {
						a[ocoord] = maxcoord;
						b[ocoord] = mincoord;
					}

					if (left != null) {
						//All triangles should be clockwise so second is the rightmost vertex (seen from this node)
						left.Add(a);
						right.Add(b);
					}
					return true;
				}
			} else
			if (!backwards) {
				int first = -1;
				int second = -1;

				int av = GetVertexCount();
				int bv = other.GetVertexCount();

				/** \todo Maybe optimize with pa=av-1 instead of modulus... */
				for (int a = 0; a < av; a++) {
					int va = GetVertexIndex(a);
					for (int b = 0; b < bv; b++) {
						if (va == other.GetVertexIndex((b+1)%bv) && GetVertexIndex((a+1) % av) == other.GetVertexIndex(b)) {
							first = a;
							second = b;
							a = av;
							break;
						}
					}
				}

				aIndex = first;
				bIndex = second;

				if (first != -1) {
					if (left != null) {
						//All triangles should be clockwise so second is the rightmost vertex (seen from this node)
						left.Add(GetVertex(first));
						right.Add(GetVertex((first+1)%av));
					}
				}
			}

			return true;
		}

		public override float SurfaceArea () {
			// TODO: This is the area in XZ space, use full 3D space for higher correctness maybe?
			var holder = GetNavmeshHolder(GraphIndex);

			return System.Math.Abs(VectorMath.SignedTriangleAreaTimes2XZ(holder.GetVertex(v0), holder.GetVertex(v1), holder.GetVertex(v2))) * 0.5f;
		}

		public override Vector3 RandomPointOnSurface () {
			// Find a random point inside the triangle
			// This generates uniformly distributed trilinear coordinates
			// See http://mathworld.wolfram.com/TrianglePointPicking.html
			float r1;
			float r2;

			do {
				r1 = Random.value;
				r2 = Random.value;
			} while (r1+r2 > 1);

			var holder = GetNavmeshHolder(GraphIndex);
			// Pick the point corresponding to the trilinear coordinate
			return ((Vector3)(holder.GetVertex(v1)-holder.GetVertex(v0)))*r1 + ((Vector3)(holder.GetVertex(v2)-holder.GetVertex(v0)))*r2 + (Vector3)holder.GetVertex(v0);
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.writer.Write(v0);
			ctx.writer.Write(v1);
			ctx.writer.Write(v2);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			v0 = ctx.reader.ReadInt32();
			v1 = ctx.reader.ReadInt32();
			v2 = ctx.reader.ReadInt32();
		}
	}
}
