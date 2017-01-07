using System;
using UnityEngine;

namespace Pathfinding.RVO
{
    [AddComponentMenu("Pathfinding/Local Avoidance/Square Obstacle")]
    public class RVOSquareObstacle : RVOObstacle
    {
        public int height = 1000;

        public Int2 size = new Int2(1000, 1000);

        public Int2 center = new Int2(1000, 1000);

        protected override bool StaticObstacle
        {
            get
            {
                return false;
            }
        }

        protected override bool ExecuteInEditor
        {
            get
            {
                return true;
            }
        }

        protected override bool LocalCoordinates
        {
            get
            {
                return true;
            }
        }

        protected override int Height
        {
            get
            {
                return this.height;
            }
        }

        protected override bool AreGizmosDirty()
        {
            return false;
        }

        protected override void CreateObstacles()
        {
            this.size.x = Mathf.Abs(this.size.x);
            this.size.y = Mathf.Abs(this.size.y);
            this.height = Mathf.Abs(this.height);
            Int3[] array = new Int3[]
            {
                new Int3(1, 0, -1),
                new Int3(1, 0, 1),
                new Int3(-1, 0, 1),
                new Int3(-1, 0, -1)
            };
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= new Int3(this.size.x >> 1, 0, this.size.y >> 1);
                array[i] += new Int3(this.center.x, 0, this.center.y);
            }
            base.AddObstacle(array, this.height);
        }
    }
}
