using System;
using System.Collections.Generic;
using System.Text;

namespace Pathfinding {
    /** Stores temporary node data for a single pathfinding request.
	 * Every node has one PathNode per thread used.
	 * It stores e.g G score, H score and other temporary variables needed
	 * for path calculation, but which are not part of the graph structure.
	 *
	 * \see Pathfinding.PathHandler
	 * \see https://en.wikipedia.org/wiki/A*_search_algorithm
	 */
    public class PathNode
    {
        private const uint CostMask = 268435455u;

        private const int Flag1Offset = 28;

        private const uint Flag1Mask = 268435456u;

        private const int Flag2Offset = 29;

        private const uint Flag2Mask = 536870912u;

        public GraphNode node;

        public PathNode parent;

        public ushort pathID;

        private uint flags;

        private uint g;

        private uint h;

        public uint cost
        {
            get
            {
                return this.flags & 268435455u;
            }
            set
            {
                this.flags = ((this.flags & 4026531840u) | value);
            }
        }

        public bool flag1
        {
            get
            {
                return (this.flags & 268435456u) != 0u;
            }
            set
            {
                this.flags = ((this.flags & 4026531839u) | ((!value) ? 0u : 268435456u));
            }
        }

        public bool flag2
        {
            get
            {
                return (this.flags & 536870912u) != 0u;
            }
            set
            {
                this.flags = ((this.flags & 3758096383u) | ((!value) ? 0u : 536870912u));
            }
        }

        public uint G
        {
            get
            {
                return this.g;
            }
            set
            {
                this.g = value;
            }
        }

        public uint H
        {
            get
            {
                return this.h;
            }
            set
            {
                this.h = value;
            }
        }

        public uint F
        {
            get
            {
                return this.g + this.h;
            }
        }
    }

    /** Handles thread specific path data.
	 */
    public class PathHandler
    {
        private const int BucketSizeLog2 = 10;

        private const int BucketSize = 1024;

        private const int BucketIndexMask = 1023;

        private ushort pathID;

        public readonly int threadID;

        public readonly int totalThreadCount;

        public BinaryHeapM heap = new BinaryHeapM(128);

        public PathNode[][] nodes = new PathNode[0][];

        private bool[] bucketNew = new bool[0];

        private bool[] bucketCreated = new bool[0];

        private Stack<PathNode[]> bucketCache = new Stack<PathNode[]>();

        private int filledBuckets;

        public readonly StringBuilder DebugStringBuilder = new StringBuilder();

        public ushort PathID
        {
            get
            {
                return this.pathID;
            }
        }

        public PathHandler(int threadID, int totalThreadCount)
        {
            this.threadID = threadID;
            this.totalThreadCount = totalThreadCount;
        }

        public void PushNode(PathNode node)
        {
            this.heap.Add(node);
        }

        public PathNode PopNode()
        {
            return this.heap.Remove();
        }

        public BinaryHeapM GetHeap()
        {
            return this.heap;
        }

        public void RebuildHeap()
        {
            this.heap.Rebuild();
        }

        public bool HeapEmpty()
        {
            return this.heap.numberOfItems <= 0;
        }

        public void InitializeForPath(Path p)
        {
            this.pathID = p.pathID;
            this.heap.Clear();
        }

        public void DestroyNode(GraphNode node)
        {
            PathNode pathNode = this.GetPathNode(node);
            pathNode.node = null;
            pathNode.parent = null;
        }

        public void InitializeNode(GraphNode node)
        {
            int nodeIndex = node.NodeIndex;
            int num = nodeIndex >> 10;
            int num2 = nodeIndex & 1023;
            if (num >= this.nodes.Length)
            {
                PathNode[][] array = new PathNode[Math.Max(Math.Max(this.nodes.Length * 3 / 2, num + 1), this.nodes.Length + 2)][];
                for (int i = 0; i < this.nodes.Length; i++)
                {
                    array[i] = this.nodes[i];
                }
                bool[] array2 = new bool[array.Length];
                for (int j = 0; j < this.nodes.Length; j++)
                {
                    array2[j] = this.bucketNew[j];
                }
                bool[] array3 = new bool[array.Length];
                for (int k = 0; k < this.nodes.Length; k++)
                {
                    array3[k] = this.bucketCreated[k];
                }
                this.nodes = array;
                this.bucketNew = array2;
                this.bucketCreated = array3;
            }
            if (this.nodes[num] == null)
            {
                PathNode[] array4;
                if (this.bucketCache.Count > 0)
                {
                    array4 = this.bucketCache.Pop();
                }
                else
                {
                    array4 = new PathNode[1024];
                    for (int l = 0; l < 1024; l++)
                    {
                        array4[l] = new PathNode();
                    }
                }
                this.nodes[num] = array4;
                if (!this.bucketCreated[num])
                {
                    this.bucketNew[num] = true;
                    this.bucketCreated[num] = true;
                }
                this.filledBuckets++;
            }
            PathNode pathNode = this.nodes[num][num2];
            pathNode.node = node;
        }

        public PathNode GetPathNode(int nodeIndex)
        {
            return this.nodes[nodeIndex >> 10][nodeIndex & 1023];
        }

        public PathNode GetPathNode(GraphNode node)
        {
            int nodeIndex = node.NodeIndex;
            return this.nodes[nodeIndex >> 10][nodeIndex & 1023];
        }

        public void ClearPathIDs()
        {
            for (int i = 0; i < this.nodes.Length; i++)
            {
                PathNode[] array = this.nodes[i];
                if (this.nodes[i] != null)
                {
                    for (int j = 0; j < 1024; j++)
                    {
                        array[j].pathID = 0;
                    }
                }
            }
        }
    }
}