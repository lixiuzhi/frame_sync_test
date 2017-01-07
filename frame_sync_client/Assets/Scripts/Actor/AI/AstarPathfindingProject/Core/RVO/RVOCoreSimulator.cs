using System;
using System.Collections.Generic;

namespace Pathfinding.RVO
{
    public enum RVOLayer
    {
        DefaultAgent = 1,
        DefaultObstacle = 2,
        Layer2 = 4,
        Layer3 = 8,
        Layer4 = 16,
        Layer5 = 32,
        Layer6 = 64,
        Layer7 = 128,
        Layer8 = 256,
        Layer9 = 512,
        Layer10 = 1024,
        Layer11 = 2048,
        Layer12 = 4096,
        Layer13 = 8192,
        Layer14 = 16384,
        Layer15 = 32768,
        Layer16 = 65536,
        Layer17 = 131072,
        Layer18 = 262144,
        Layer19 = 524288,
        Layer20 = 1048576,
        Layer21 = 2097152,
        Layer22 = 4194304,
        Layer23 = 8388608,
        Layer24 = 16777216,
        Layer25 = 33554432,
        Layer26 = 67108864,
        Layer27 = 134217728,
        Layer28 = 268435456,
        Layer29 = 536870912,
        Layer30 = 1073741824
    }
    public interface IAgent
    {
        Int3 InterpolatedPosition
        {
            get;
        }

        Int3 Position
        {
            get;
        }

        Int3 DesiredVelocity
        {
            get;
            set;
        }

        Int3 Velocity
        {
            get;
            set;
        }

        bool Locked
        {
            get;
            set;
        }

        Int1 Radius
        {
            get;
            set;
        }

        Int1 Height
        {
            get;
            set;
        }

        Int1 MaxSpeed
        {
            get;
            set;
        }

        Int1 NeighbourDist
        {
            get;
            set;
        }

        int AgentTimeHorizon
        {
            get;
            set;
        }

        int ObstacleTimeHorizon
        {
            get;
            set;
        }

        RVOLayer Layer
        {
            get;
            set;
        }

        RVOLayer CollidesWith
        {
            get;
            set;
        }

        bool DebugDraw
        {
            get;
            set;
        }

        int MaxNeighbours
        {
            get;
            set;
        }

        List<ObstacleVertex> NeighbourObstacles
        {
            get;
        }

        void SetYPosition(Int1 yCoordinate);

        void Teleport(Int3 pos);
    }
}
