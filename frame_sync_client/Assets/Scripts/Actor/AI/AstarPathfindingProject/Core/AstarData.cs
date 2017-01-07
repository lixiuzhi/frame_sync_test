using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Pathfinding.WindowsStore;
#if UNITY_WINRT && !UNITY_EDITOR
//using MarkerMetro.Unity.WinLegacy.IO;
//using MarkerMetro.Unity.WinLegacy.Reflection;
#endif

namespace Pathfinding {
	[System.Serializable]
	/** Stores the navigation graphs for the A* Pathfinding System.
	 * \ingroup relevant
	 *
	 * An instance of this class is assigned to AstarPath.astarData, from it you can access all graphs loaded through the #graphs variable.\n
	 * This class also handles a lot of the high level serialization.
	 */
	public class AstarData {
		/** Shortcut to AstarPath.active */
		public static AstarPath active {
			get {
				return AstarPath.active;
			}
		}

		#region Fields
		/** Shortcut to the first NavMeshGraph.
		 * Updated at scanning time
		 */
		public NavMeshGraph navmesh { get; private set; }
         
		/** Shortcut to the first RecastGraph.
		 * Updated at scanning time.
		 * \astarpro
		 */
		public RecastGraph recastGraph { get; set; }

		/** All supported graph types.
		 * Populated through reflection search
		 */
		public System.Type[] graphTypes { get; private set; }

#if ASTAR_FAST_NO_EXCEPTIONS || UNITY_WINRT || UNITY_WEBGL
		/** Graph types to use when building with Fast But No Exceptions for iPhone.
		 * If you add any custom graph types, you need to add them to this hard-coded list.
		 */
		public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
 
#if !ASTAR_NO_POINT_GRAPH
			typeof(PointGraph),
#endif
			typeof(NavMeshGraph),
			typeof(RecastGraph),
			typeof(LayerGridGraph)
		};
#endif

		/** All graphs this instance holds.
		 * This will be filled only after deserialization has completed.
		 * May contain null entries if graph have been removed.
		 */
		[System.NonSerialized]
		public NavGraph[] graphs = new NavGraph[0];

		//Serialization Settings

		/** Serialized data for all graphs and settings.
		 * Stored as a base64 encoded string because otherwise Unity's Undo system would sometimes corrupt the byte data (because it only stores deltas).
		 *
		 * This can be accessed as a byte array from the #data property.
		 *
		 * \since 3.6.1
		 */
		[SerializeField]
		string dataString;

		/** Data from versions from before 3.6.1.
		 * Used for handling upgrades
		 * \since 3.6.1
		 */
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("data")]
		private byte[] upgradeData;

		/** Serialized data for all graphs and settings */
		private byte[] data {
			get {
				// Handle upgrading from earlier versions than 3.6.1
				if (upgradeData != null && upgradeData.Length > 0) {
					data = upgradeData;
					upgradeData = null;
				}
				return dataString != null ? System.Convert.FromBase64String(dataString) : null;
			}
			set {
				dataString = value != null ? System.Convert.ToBase64String(value) : null;
			}
		}

		/** Backup data if deserialization failed.
		 */
		public byte[] data_backup;

		/** Serialized data for cached startup.
		 * If set, on start the graphs will be deserialized from this file.
		 */
		public TextAsset file_cachedStartup;

		/** Serialized data for cached startup.
		 *
		 * \deprecated Deprecated since 3.6, AstarData.file_cachedStartup is now used instead
		 */
		public byte[] data_cachedStartup;

		/** Should graph-data be cached.
		 * Caching the startup means saving the whole graphs, not only the settings to an internal array (#data_cachedStartup) which can
		 * be loaded faster than scanning all graphs at startup. This is setup from the editor.
		 */
		[SerializeField]
		public bool cacheStartup;

        //End Serialization Settings

        #endregion


        public GraphNodeRasterizer rasterizer;

        public byte[] GetData () {
			return data;
		}

		public void SetData (byte[] data) {
			this.data = data;
		}

		/** Loads the graphs from memory, will load cached graphs if any exists */
		public void Awake () {
			graphs = new NavGraph[0];

			if (cacheStartup && file_cachedStartup != null) {
				LoadFromCache();
			} else {
				DeserializeGraphs();
			}
		}

        /// <summary>
        /// ≥ı ºªØgraphNode’§∏Ò∆˜
        /// </summary>
        public void InitRasterizer(int inCellSize =4000)
        {
            if (this.graphs == null || this.graphs.Length == 0)
            {
                return;
            }
            rasterizer = new GraphNodeRasterizer();
            Int2 min = new Int2(2147483647, 2147483647);
            Int2 max = new Int2(-2147483648, -2147483648);

            for (int j = 0; j < this.graphs.Length; j++)
            {
                RecastGraph recastGraph2 = this.graphs[j] as RecastGraph;
                if (recastGraph2 == null)
                {
                    return;
                }
                recastGraph2.GetNodes(delegate (GraphNode node)
                {
                    TriangleMeshNode triangleMeshNode = node as TriangleMeshNode;
                    if (triangleMeshNode != null)
                    {
                        Int2 xz = triangleMeshNode.GetVertex(0).xz;
                        Int2 xz2 = triangleMeshNode.GetVertex(1).xz;
                        Int2 xz3 = triangleMeshNode.GetVertex(2).xz;
                        min.Min(ref xz);
                        min.Min(ref xz2);
                        min.Min(ref xz3);
                        max.Max(ref xz);
                        max.Max(ref xz2);
                        max.Max(ref xz3);
                    }
                    return true;
                });
            }
            rasterizer.Init(min, max.x - min.x, max.y - min.y, inCellSize);

            for (int k = 0; k < this.graphs.Length; k++)
            {
                RecastGraph recastGraph3 = this.graphs[k] as RecastGraph;
                if (recastGraph3 != null)
                {
                    recastGraph3.GetNodes(delegate (GraphNode node)
                    {
                        TriangleMeshNode triangleMeshNode = node as TriangleMeshNode;
                        if (triangleMeshNode != null)
                        {
                            Int2 xz = triangleMeshNode.GetVertex(0).xz;
                            Int2 xz2 = triangleMeshNode.GetVertex(1).xz;
                            Int2 xz3 = triangleMeshNode.GetVertex(2).xz;
                            rasterizer.AddTriangle(ref xz, ref xz2, ref xz3, triangleMeshNode);
                        }
                        return true;
                    });
                }
            } 
        }

        public TriangleMeshNode GetLocatedByRasterizer(Int3 position)
        {
            TriangleMeshNode result = null;
            if (this.rasterizer != null)
            {
                List<object> located = this.rasterizer.GetLocated(position);
                if (located != null)
                {
                    for (int i = 0; i < located.Count; i++)
                    {
                        TriangleMeshNode triangleMeshNode = located[i] as TriangleMeshNode;
                        if (triangleMeshNode == null)
                        {
                            break;
                        }
                        Int3 a;
                        Int3 b;
                        Int3 c;
                        triangleMeshNode.GetPoints(out a, out b, out c);

                        AStarDebug.DrawTriangle(0,a,b,c);
                        if (Polygon.ContainsPoint(a, b, c, position))
                        {
                            result = triangleMeshNode;
                            break;
                        }
                    }
                }
            }
            return result;
        }


        public TriangleMeshNode GetNearestByRasterizer(Int3 position, out Int3 clampedPosition)
        {
            clampedPosition = Int3.zero;
            if (this.rasterizer == null)
            {
                return null;
            }
            TriangleMeshNode triangleMeshNode = this.GetLocatedByRasterizer(position);
            if (triangleMeshNode != null)
            {
                clampedPosition = position;
                return triangleMeshNode;
            }
            triangleMeshNode = this.FindNearestByRasterizer(position, -1);
            if (triangleMeshNode == null)
            {
                return null;
            }
            clampedPosition = triangleMeshNode.ClosestPointOnNodeXZ(position);
            return triangleMeshNode;
        }


        public TriangleMeshNode FindNearestByRasterizer(Int3 position, int maxRange = -1)
        {
            if (this.rasterizer == null)
            {
                return null;
            }
            int num;
            int num2;
            this.rasterizer.GetCellPosClamped(out num, out num2, position);
            long num3 = 9223372036854775807L;
            TriangleMeshNode triangleMeshNode = null;
            List<object> objs = this.rasterizer.GetObjs(num, num2);
            if (this.getNearest(position, objs, ref num3, ref triangleMeshNode))
            {
                return triangleMeshNode;
            }
            int i = 1;
            int num4 = Mathf.Max(this.rasterizer.numCellsX, this.rasterizer.numCellsY);
            if (maxRange == -1)
            {
                maxRange = num4;
            }
            else
            {
                maxRange = Mathf.Clamp(maxRange, 1, num4);
            }
            while (i < maxRange)
            {
                int num5 = Mathf.Max(num - i, 0);
                int num6 = Mathf.Min(num + i, this.rasterizer.numCellsX - 1);
                int num7 = Mathf.Max(num2 - i, 0);
                int num8 = Mathf.Min(num2 + i, this.rasterizer.numCellsY - 1);
                if (num - i == num5)
                {
                    for (int j = num7; j <= num8; j++)
                    {
                        this.getNearest(position, this.rasterizer.GetObjs(num5, j), ref num3, ref triangleMeshNode);
                    }
                }
                if (num + i == num6)
                {
                    for (int k = num7; k <= num8; k++)
                    {
                        this.getNearest(position, this.rasterizer.GetObjs(num6, k), ref num3, ref triangleMeshNode);
                    }
                }
                if (num2 - i == num7)
                {
                    for (int l = num5 + 1; l < num6; l++)
                    {
                        this.getNearest(position, this.rasterizer.GetObjs(l, num7), ref num3, ref triangleMeshNode);
                    }
                }
                if (num2 + i == num8)
                {
                    for (int m = num5 + 1; m < num6; m++)
                    {
                        this.getNearest(position, this.rasterizer.GetObjs(m, num8), ref num3, ref triangleMeshNode);
                    }
                }
                if (triangleMeshNode != null)
                {
                    break;
                }
                i++;
            }
            return triangleMeshNode;
        }

        private bool getNearest(Int3 position, List<object> objs, ref long minDist, ref TriangleMeshNode nearestNode)
        {
            if (objs == null || objs.Count == 0)
            {
                return false;
            }
            bool result = false;
            for (int i = 0; i < objs.Count; i++)
            {
                TriangleMeshNode triangleMeshNode = objs[i] as TriangleMeshNode;
                if (triangleMeshNode == null)
                {
                    return false;
                }
                long num = position.XZSqrMagnitude(triangleMeshNode.position);
                if (num < minDist)
                {
                    minDist = num;
                    nearestNode = triangleMeshNode;
                    result = true;
                }
            }
            return result;
        } 

        public TriangleMeshNode IntersectByRasterizer(Int3 start, Int3 end, out int edge)
        {
            edge = -1;
            if (this.rasterizer == null)
            {
                return null;
            }
            int num = end.x - start.x;
            int num2 = end.z - start.z;
            int num3 = start.x - this.rasterizer.origin.x;
            int num4 = start.z - this.rasterizer.origin.y;
            int num5 = Mathf.Abs(num);
            int num6 = num3 % this.rasterizer.cellSize;
            int num7 = (num <= 0) ? (-num6 - 1) : (this.rasterizer.cellSize - num6);
            int num8 = Mathf.Abs(num7);
            int num9 = this.rasterizer.numCellsX * this.rasterizer.cellSize;
            int num10 = this.rasterizer.numCellsY * this.rasterizer.cellSize;
            int num11 = num3;
            int num12 = num4;
            while (num5 >= 0 && num11 >= 0 && num11 < num9)
            {
                int gridX = num11 / this.rasterizer.cellSize;
                int num13 = Mathf.Abs((num == 0) ? num2 : IntMath.Divide(num2 * num7, num));
                int num14 = num12 % this.rasterizer.cellSize;
                int num15 = (num2 < 0) ? (-num14 - 1) : (this.rasterizer.cellSize - num14);
                int num16 = Mathf.Abs(num15);
                int num17 = num12;
                while (num13 >= 0 && num12 >= 0 && num12 < num10)
                {
                    int gridY = num12 / this.rasterizer.cellSize;
                    TriangleMeshNode triangleMeshNode = this.checkObjIntersects(ref edge, start, end, gridX, gridY);
                    if (triangleMeshNode != null)
                    {
                        return triangleMeshNode;
                    }
                    num12 += num15;
                    num13 -= num16;
                    num15 = ((num2 < 0) ? (-this.rasterizer.cellSize) : this.rasterizer.cellSize);
                    num16 = this.rasterizer.cellSize;
                }
                num11 += num7;
                num5 -= num8;
                num7 = ((num < 0) ? (-this.rasterizer.cellSize) : this.rasterizer.cellSize);
                num8 = this.rasterizer.cellSize;
                if (num != 0)
                {
                    num12 = (num17 * num + num2 * num7) / num;
                }
            }
            return null;
        }


        private TriangleMeshNode checkObjIntersects(ref int edge, Int3 start, Int3 end, int gridX, int gridY)
        {
            List<object> objs = this.rasterizer.GetObjs(gridX, gridY);
            if (objs == null || objs.Count == 0)
            {
                return null;
            }
            Int3[] array = new Int3[3];
            TriangleMeshNode triangleMeshNode = null;
            int num = -1;
            long num2 = 9223372036854775807L;
            for (int i = 0; i < objs.Count; i++)
            {
                TriangleMeshNode triangleMeshNode2 = objs[i] as TriangleMeshNode;
                triangleMeshNode2.GetPoints(out array[0], out array[1], out array[2]);
                for (int j = 0; j < 3; j++)
                {
                    int num3 = j;
                    int num4 = (j + 1) % 3;
                    if (Polygon.Intersects(array[num3], array[num4], start, end))
                    {
                        bool flag;
                        Int3 vInt = Polygon.IntersectionPoint(ref array[num3], ref array[num4], ref start, ref end, out flag);
         
                        long num5 = start.XZSqrMagnitude(ref vInt);
                        if (num5 < num2)
                        {
                            num2 = num5;
                            triangleMeshNode = triangleMeshNode2;
                            num = j;
                        }
                    }
                }
            }
            if (num != -1 && triangleMeshNode != null)
            {
                edge = num;
                return triangleMeshNode;
            }
            return null;
        }
        public bool CheckSegmentIntersects(Int3 start, Int3 end, int gridX, int gridY, out Int3 outPoint, out TriangleMeshNode nearestNode)
        {
            List<object> objs = this.rasterizer.GetObjs(gridX, gridY);
            outPoint = end;
            nearestNode = null;
            if (objs == null || objs.Count == 0)
            {
                return false;
            }
            Int3[] array = new Int3[3];
            bool result = false;
            long num = 9223372036854775807L;
            for (int i = 0; i < objs.Count; i++)
            {
                TriangleMeshNode triangleMeshNode = objs[i] as TriangleMeshNode;
                triangleMeshNode.GetPoints(out array[0], out array[1], out array[2]);
                for (int j = 0; j < 3; j++)
                {
                    int num2 = j;
                    int num3 = (j + 1) % 3;
                    bool flag = false;
                    Int3 vInt = Polygon.SegmentIntersectionPoint(array[num2], array[num3], start, end, out flag);
                    if (flag)
                    {
                        long num4 = start.XZSqrMagnitude(ref vInt);
                        if (num4 < num)
                        {
                            nearestNode = triangleMeshNode;
                            num = num4;
                            outPoint = vInt;
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        /** Updates shortcuts to the first graph of different types.
		 * Hard coding references to some graph types is not really a good thing imo. I want to keep it dynamic and flexible.
		 * But these references ease the use of the system, so I decided to keep them.\n
		 */
        public void UpdateShortcuts () {
			navmesh = (NavMeshGraph)FindGraphOfType(typeof(NavMeshGraph));
 
			recastGraph = (RecastGraph)FindGraphOfType(typeof(RecastGraph));
		}

		/** Load from data from #file_cachedStartup */
		public void LoadFromCache () {
			AstarPath.active.BlockUntilPathQueueBlocked();
			if (file_cachedStartup != null) {
				var bytes = file_cachedStartup.bytes;
				DeserializeGraphs(bytes);

				GraphModifier.TriggerEvent(GraphModifier.EventType.PostCacheLoad);
			} else {
				Debug.LogError("Can't load from cache since the cache is empty");
			}
		}

		#region Serialization

		/** Serializes all graphs settings to a byte array.
		 * \see DeserializeGraphs(byte[])
		 */
		public byte[] SerializeGraphs () {
			return SerializeGraphs(Pathfinding.Serialization.SerializeSettings.Settings);
		}

		/** Serializes all graphs settings and optionally node data to a byte array.
		 * \see DeserializeGraphs(byte[])
		 * \see Pathfinding.Serialization.SerializeSettings
		 */
		public byte[] SerializeGraphs (Pathfinding.Serialization.SerializeSettings settings) {
			uint checksum;

			return SerializeGraphs(settings, out checksum);
		}

		/** Main serializer function.
		 * Serializes all graphs to a byte array
		 * A similar function exists in the AstarPathEditor.cs script to save additional info */
		public byte[] SerializeGraphs (Pathfinding.Serialization.SerializeSettings settings, out uint checksum) {
			AstarPath.active.BlockUntilPathQueueBlocked();

			var sr = new Pathfinding.Serialization.AstarSerializer(this, settings);
			sr.OpenSerialize();
			SerializeGraphsPart(sr);
			byte[] bytes = sr.CloseSerialize();
			checksum = sr.GetChecksum();
	#if ASTARDEBUG
			Debug.Log("Got a whole bunch of data, "+bytes.Length+" bytes");
	#endif
			return bytes;
		}

		/** Serializes common info to the serializer.
		 * Common info is what is shared between the editor serialization and the runtime serializer.
		 * This is mostly everything except the graph inspectors which serialize some extra data in the editor
		 */
		public void SerializeGraphsPart (Pathfinding.Serialization.AstarSerializer sr) {
			sr.SerializeGraphs(graphs);
			sr.SerializeExtraInfo();
		}

		/** Deserializes graphs from #data */
		public void DeserializeGraphs () {
			if (data != null) {
				DeserializeGraphs(data);
			}
		}

		/** Destroys all graphs and sets graphs to null */
		void ClearGraphs () {
			if (graphs == null) return;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null) graphs[i].OnDestroy();
			}
			graphs = null;
			UpdateShortcuts();
		}

		public void OnDestroy () {
			ClearGraphs();
		}

		/** Deserializes graphs from the specified byte array.
		 * An error will be logged if deserialization fails.
		 */
		public void DeserializeGraphs (byte[] bytes) {
			AstarPath.active.BlockUntilPathQueueBlocked();
			ClearGraphs();
			DeserializeGraphsAdditive(bytes);
		}

		/** Deserializes graphs from the specified byte array additively.
		 * An error will be logged if deserialization fails.
		 * This function will add loaded graphs to the current ones.
		 */
		public void DeserializeGraphsAdditive (byte[] bytes) {
			AstarPath.active.BlockUntilPathQueueBlocked();

			try {
				if (bytes != null) {
					var sr = new Pathfinding.Serialization.AstarSerializer(this);

					if (sr.OpenDeserialize(bytes)) {
						DeserializeGraphsPartAdditive(sr);
						sr.CloseDeserialize();
						UpdateShortcuts();
					} else {
						Debug.Log("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
					}
				} else {
					throw new System.ArgumentNullException("bytes");
				}
				active.VerifyIntegrity();
			} catch (System.Exception e) {
				Debug.LogError("Caught exception while deserializing data.\n"+e);
				graphs = new NavGraph[0];
				data_backup = bytes;
			}
		}

		/** Deserializes common info.
		 * Common info is what is shared between the editor serialization and the runtime serializer.
		 * This is mostly everything except the graph inspectors which serialize some extra data in the editor
		 *
		 * In most cases you should use the DeserializeGraphs or DeserializeGraphsAdditive method instead.
		 */
		public void DeserializeGraphsPart (Pathfinding.Serialization.AstarSerializer sr) {
			ClearGraphs();
			DeserializeGraphsPartAdditive(sr);
		}

		/** Deserializes common info additively
		 * Common info is what is shared between the editor serialization and the runtime serializer.
		 * This is mostly everything except the graph inspectors which serialize some extra data in the editor
		 *
		 * In most cases you should use the DeserializeGraphs or DeserializeGraphsAdditive method instead.
		 */
		public void DeserializeGraphsPartAdditive (Pathfinding.Serialization.AstarSerializer sr) {
			if (graphs == null) graphs = new NavGraph[0];

			var gr = new List<NavGraph>(graphs);

			// Set an offset so that the deserializer will load
			// the graphs with the correct graph indexes
			sr.SetGraphIndexOffset(gr.Count);

			gr.AddRange(sr.DeserializeGraphs());
			graphs = gr.ToArray();

			sr.DeserializeExtraInfo();

			//Assign correct graph indices.
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] == null) continue;
				graphs[i].GetNodes(node => {
					node.GraphIndex = (uint)i;
					return true;
				});
			}

			for (int i = 0; i < graphs.Length; i++) {
				for (int j = i+1; j < graphs.Length; j++) {
					if (graphs[i] != null && graphs[j] != null && graphs[i].guid == graphs[j].guid) {
						Debug.LogWarning("Guid Conflict when importing graphs additively. Imported graph will get a new Guid.\nThis message is (relatively) harmless.");
						graphs[i].guid = Pathfinding.Util.Guid.NewGuid();
						break;
					}
				}
			}

			sr.PostDeserialization();
		}

		#endregion

		/** Find all graph types supported in this build.
		 * Using reflection, the assembly is searched for types which inherit from NavGraph. */
		public void FindGraphTypes () {
#if !ASTAR_FAST_NO_EXCEPTIONS && !UNITY_WINRT && !UNITY_WEBGL
			var asm = WindowsStoreCompatibility.GetTypeInfo(typeof(AstarPath)).Assembly;

			System.Type[] types = asm.GetTypes();

			var graphList = new List<System.Type>();

			foreach (System.Type type in types) {
#if NETFX_CORE && !UNITY_EDITOR
				System.Type baseType = type.GetTypeInfo().BaseType;
#else
				System.Type baseType = type.BaseType;
#endif
				while (baseType != null) {
					if (System.Type.Equals(baseType, typeof(NavGraph))) {
						graphList.Add(type);

						break;
					}

#if NETFX_CORE && !UNITY_EDITOR
					baseType = baseType.GetTypeInfo().BaseType;
#else
					baseType = baseType.BaseType;
#endif
				}
			}

			graphTypes = graphList.ToArray();

#if ASTARDEBUG
			Debug.Log("Found "+graphTypes.Length+" graph types");
#endif
#else
			graphTypes = DefaultGraphTypes;
#endif
		}

		#region GraphCreation
		/**
		 * \returns A System.Type which matches the specified \a type string. If no mathing graph type was found, null is returned
		 *
		 * \deprecated
		 */
		[System.Obsolete("If really necessary. Use System.Type.GetType instead.")]
		public System.Type GetGraphType (string type) {
			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					return graphTypes[i];
				}
			}
			return null;
		}

		/** Creates a new instance of a graph of type \a type. If no matching graph type was found, an error is logged and null is returned
		 * \returns The created graph
		 * \see CreateGraph(System.Type)
		 *
		 * \deprecated
		 */
		[System.Obsolete("Use CreateGraph(System.Type) instead")]
		public NavGraph CreateGraph (string type) {
			Debug.Log("Creating Graph of type '"+type+"'");

			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					return CreateGraph(graphTypes[i]);
				}
			}
			Debug.LogError("Graph type ("+type+") wasn't found");
			return null;
		}

		/** Creates a new graph instance of type \a type
		 * \see CreateGraph(string)
		 */
		public NavGraph CreateGraph (System.Type type) {
			var g = System.Activator.CreateInstance(type) as NavGraph;

			g.active = active;
			return g;
		}

		/** Adds a graph of type \a type to the #graphs array
		 *
		 * \deprecated
		 */
		[System.Obsolete("Use AddGraph(System.Type) instead")]
		public NavGraph AddGraph (string type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("No NavGraph of type '"+type+"' could be found");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/** Adds a graph of type \a type to the #graphs array */
		public NavGraph AddGraph (System.Type type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (System.Type.Equals(graphTypes[i], type)) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("No NavGraph of type '"+type+"' could be found, "+graphTypes.Length+" graph types are avaliable");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/** Adds the specified graph to the #graphs array */
		public void AddGraph (NavGraph graph) {
			// Make sure to not interfere with pathfinding
			AstarPath.active.BlockUntilPathQueueBlocked();

			// Try to fill in an empty position
			bool foundEmpty = false;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] == null) {
					graphs[i] = graph;
					graph.graphIndex = (uint)i;
					foundEmpty = true;
				}
			}

			if (!foundEmpty) {
				if (graphs != null && graphs.Length >= GraphNode.MaxGraphIndex) {
					throw new System.Exception("Graph Count Limit Reached. You cannot have more than " + GraphNode.MaxGraphIndex + " graphs.");
				}

				// Add a new entry to the list
				var ls = new List<NavGraph>(graphs ?? new NavGraph[0]);
				ls.Add(graph);
				graphs = ls.ToArray();
				graph.graphIndex = (uint)(graphs.Length-1);
			}

			UpdateShortcuts();
			graph.active = active;
			graph.Awake();
		}

		/** Removes the specified graph from the #graphs array and Destroys it in a safe manner.
		 * To avoid changing graph indices for the other graphs, the graph is simply nulled in the array instead
		 * of actually removing it from the array.
		 * The empty position will be reused if a new graph is added.
		 *
		 * \returns True if the graph was sucessfully removed (i.e it did exist in the #graphs array). False otherwise.
		 *
		 * \version Changed in 3.2.5 to call SafeOnDestroy before removing
		 * and nulling it in the array instead of removing the element completely in the #graphs array.
		 */
		public bool RemoveGraph (NavGraph graph) {
			// Make sure all graph updates and other callbacks are done
			active.FlushWorkItemsInternal(false);

			// Make sure the pathfinding threads are stopped
			// If we don't wait until pathfinding that is potentially running on
			// this graph right now we could end up with NullReferenceExceptions
			active.BlockUntilPathQueueBlocked();

			graph.OnDestroy();

			int i = System.Array.IndexOf(graphs, graph);
			if (i == -1) return false;

			graphs[i] = null;
			UpdateShortcuts();
			return true;
		}

		#endregion

		#region GraphUtility

		/** Returns the graph which contains the specified node.
		 * The graph must be in the #graphs array.
		 *
		 * \returns Returns the graph which contains the node. Null if the graph wasn't found
		 */
		public static NavGraph GetGraph (GraphNode node) {
			if (node == null) return null;

			AstarPath script = AstarPath.active;
			if (script == null) return null;

			AstarData data = script.astarData;
			if (data == null || data.graphs == null) return null;

			uint graphIndex = node.GraphIndex;

			if (graphIndex >= data.graphs.Length) {
				return null;
			}

			return data.graphs[(int)graphIndex];
		}

		/** Returns the first graph of type \a type found in the #graphs array. Returns null if none was found */
		public NavGraph FindGraphOfType (System.Type type) {
			if (graphs != null) {
				for (int i = 0; i < graphs.Length; i++) {
					if (graphs[i] != null && System.Type.Equals(graphs[i].GetType(), type)) {
						return graphs[i];
					}
				}
			}
			return null;
		}

		/** Loop through this function to get all graphs of type 'type'
		 * \code foreach (GridGraph graph in AstarPath.astarData.FindGraphsOfType (typeof(GridGraph))) {
		 *  //Do something with the graph
		 * } \endcode
		 * \see AstarPath.RegisterSafeNodeUpdate */
		public IEnumerable FindGraphsOfType (System.Type type) {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && System.Type.Equals(graphs[i].GetType(), type)) {
					yield return graphs[i];
				}
			}
		}

		/** All graphs which implements the UpdateableGraph interface
		 * \code foreach (IUpdatableGraph graph in AstarPath.astarData.GetUpdateableGraphs ()) {
		 *  //Do something with the graph
		 * } \endcode
		 * \see AstarPath.RegisterSafeNodeUpdate
		 * \see Pathfinding.IUpdatableGraph */
		public IEnumerable GetUpdateableGraphs () {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is IUpdatableGraph) {
					yield return graphs[i];
				}
			}
		}

		/** All graphs which implements the UpdateableGraph interface
		 * \code foreach (IRaycastableGraph graph in AstarPath.astarData.GetRaycastableGraphs ()) {
		 *  //Do something with the graph
		 * } \endcode
		 * \see Pathfinding.IRaycastableGraph
		 * \deprecated Deprecated because it is not used by the package internally and the use cases are few. Iterate through the #graphs array instead.
		 */
		[System.Obsolete("Obsolete because it is not used by the package internally and the use cases are few. Iterate through the graphs array instead.")]
		public IEnumerable GetRaycastableGraphs () {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is IRaycastableGraph) {
					yield return graphs[i];
				}
			}
		}

		/** Gets the index of the NavGraph in the #graphs array */
		public int GetGraphIndex (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException("graph");

			var index = -1;
			if (graphs != null) {
				index = System.Array.IndexOf(graphs, graph);
				if (index == -1) Debug.LogError("Graph doesn't exist");
			}
			return index;
		}

		#endregion
	}
}
