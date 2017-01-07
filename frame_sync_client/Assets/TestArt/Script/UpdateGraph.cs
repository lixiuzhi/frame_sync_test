using UnityEngine;
using System.Collections;
using Pathfinding;

/// <summary>
/// 这个行不通，继续
/// </summary>
public class UpdateGraph : MonoBehaviour
{

    Bounds bounds;
    public int disableTag = 1;
    public int activeTag = 2;
    public void Awake()
    {
        bounds = GetComponent<Collider>().bounds;
    }

    void OnEnable()
    {
        UpdateBound(activeTag);
    }
    void OnDisable()
    {
        UpdateBound(disableTag);
    }

    void UpdateBound(int tag)
    {
        GraphUpdateObject guo = new GraphUpdateObject(bounds);

        // There are only 32 tags
        if (tag > 31) { Debug.LogError("tag > 31"); return; }

        guo.modifyTag = true;
        guo.setTag = tag;
        guo.updatePhysics = false;

        AstarPath.active.UpdateGraphs(guo);
    }
}
