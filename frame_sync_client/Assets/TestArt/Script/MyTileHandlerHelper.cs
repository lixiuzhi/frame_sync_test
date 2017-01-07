using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;

namespace Pathfinding
{
    public class MyTileHandlerHelper : SingletonTemplate<MyTileHandlerHelper>
    {
        MyTileHandler handler;

        readonly List<Bounds> forcedReloadBounds = new List<Bounds>();

        void Clear()
        {
            NavmeshCut.OnDestroyCallback -= HandleOnDestroyCallback;
        }

        /** Discards all pending updates caused by moved or modified navmesh cuts */
        public void DiscardPending()
        {
            List<NavmeshCut> cuts = NavmeshCut.GetAll();
            for (int i = 0; i < cuts.Count; i++)
            {
                if (cuts[i].RequiresUpdate())
                {
                    cuts[i].NotifyUpdated();
                }
            }
        }

        public void Start()
        {
            NavmeshCut.OnDestroyCallback += HandleOnDestroyCallback;

            if (handler == null)
            {
                if (AstarPath.active == null || AstarPath.active.astarData.recastGraph == null)
                {
                    Debug.LogWarning("No AstarPath object in the scene or no RecastGraph on that AstarPath object");
                }

                var graph = AstarPath.active.astarData.recastGraph;
                handler = new MyTileHandler(graph);
                handler.CreateTileTypesFromGraph();
            }
        }

        /** Called when a NavmeshCut is destroyed */
        void HandleOnDestroyCallback(NavmeshCut obj)
        {
            forcedReloadBounds.Add(obj.LastBounds);
        }

        private bool ShouldRebuild()
        {
            List<NavmeshCut> all = NavmeshCut.GetAll();
            if (this.forcedReloadBounds.Count != 0)
            {
                return true;
            }
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].RequiresUpdate())
                {
                    return true;
                }
            }
            return false;
        }
        private void CreateHandlers(List<NavmeshCut> cuts)
        {
            if (this.handler != null)
            {
                return;
            }
            AstarPath active = AstarPath.active;
            if (active == null || active.astarData == null || active.astarData.recastGraph == null)
            {
                return;
            }
            if (active.astarData == null)
            {
                active.astarData = new AstarData();
                active.astarData.graphs = new NavGraph[]
                {
                    active.astarData.recastGraph
                };
            }
            handler = new MyTileHandler(active.astarData.recastGraph);
            handler.CreateTileTypesFromGraph();
        }

        public void Rebuild()
        {
            List<NavmeshCut> all = NavmeshCut.GetAll();
            List<NavmeshCut> listView = new List<NavmeshCut>();
            this.CreateHandlers(all);
            if (this.handler == null)
            {
                return;
            }
            AstarPath active = AstarPath.active;
            int num = active.astarData.graphs.Length + 1;
            for (int i = 0; i < all.Count; i++)
            {
                all[i].Check();
            }

            listView.Clear();
            for (int k = 0; k < all.Count; k++)
            {
                NavmeshCut navmeshCut = all[k];
                listView.Add(navmeshCut);
            }

            this.handler.ReloadTiles(listView);
            AstarPath.active.astarData.InitRasterizer();

            for (int l = 0; l < all.Count; l++)
            {
                if (all[l].RequiresUpdate())
                {
                    all[l].NotifyUpdated();
                }
            }
            this.forcedReloadBounds.Clear();
        }

        public void Update()
        {
            if (ShouldRebuild())
            {
                Rebuild();
            }
        }
    }
}
