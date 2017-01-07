using System;
using System.Collections.Generic;


public class ObjectGameEventSet
{
    private Dictionary<int, List<EventAction>> evtSet = new Dictionary<int, List<EventAction>>();
     
    /// <summary>
    /// 添加事件
    /// </summary>
    public void AddListener(EventID evtId, EventAction callBack)
    {
        addListener((int)evtId, callBack);
    }

    /// <summary>
    /// 添加事件
    /// </summary>
    private void addListener(int evtId, EventAction callBack)
    {
        GameEventDispatcher.Singleton.AddListener(evtId, callBack);
        if (!evtSet.ContainsKey(evtId))
            evtSet.Add(evtId, new List<EventAction>());
        if (!evtSet[evtId].Contains(callBack))
            evtSet[evtId].Add(callBack);
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    public void RemoveListener(EventID evtId, EventAction callBack)
    {
        removeListener((int)evtId, callBack);
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    private void removeListener(int evtId, EventAction callBack)
    {
        GameEventDispatcher.Singleton.RemoveListener((int)evtId, callBack);
        if (evtSet.ContainsKey(evtId))
        {
            if (evtSet[evtId].Contains(callBack))
                evtSet[evtId].Remove(callBack);
        }
    }

    public void RemoveAllListener()
    {
        foreach (int id in evtSet.Keys)
        {
            if (evtSet.ContainsKey(id))
            {
                List<EventAction> list = evtSet[id];
                foreach (EventAction act in list)
                    GameEventDispatcher.Singleton.RemoveListener(id, act);
            }
        }
        evtSet.Clear();
    }

}
