using UnityEngine;
using System.Collections;

namespace Pathfinding.RVO {
	/** One vertex in an obstacle.
	 * This is a linked list and one vertex can therefore be used to reference the whole obstacle
	 * \astarpro
	 */
	public class ObstacleVertex {
        public bool ignore;

        public Int3 position;

        public Int2 dir;

        public Int1 height;

        public RVOLayer layer;

        public bool convex;

        public bool split;

        public ObstacleVertex next;

        public ObstacleVertex prev;
    }
}
