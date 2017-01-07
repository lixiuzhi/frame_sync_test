 
using System; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public delegate void EventAction(GameEvent e);
public class GameEventDispatcher : SingletonTemplate<GameEventDispatcher>
{
    /// <summary>
    /// C#事件
    /// </summary>
    private Dictionary<Int32,EventAction> mEventHandlers;
    private Dictionary<Int32,EventAction> mEventOnceHandlers;

   
    public GameEventDispatcher()
    { 
        mEventHandlers = new Dictionary<Int32, EventAction>();
        mEventOnceHandlers = new Dictionary<Int32, EventAction>(); 
    }

 
    public void AddListener(Int32 evtType, EventAction handler)
    {
        addListener(mEventHandlers, evtType, handler);
    }
     
    public void AddListener(EventID evtType, EventAction handler)
    {
        AddListener((Int32)evtType, handler);
    }
     
    public void AddListenerOnce(Int32 evtType, EventAction handler)
    {
        addListener(mEventOnceHandlers, evtType, handler);
    }
     
    public void AddListenerOnce(EventID evtType, EventAction handler)
    {
        AddListenerOnce((Int32)evtType, handler);
    }
  
    public void RemoveListener(Int32 evtType, EventAction handler)
    {
        removeListener(mEventHandlers, evtType, handler);
        removeListener(mEventOnceHandlers, evtType, handler);
    }
     
    public void RemoveListener(EventID evtType, EventAction handler)
    {
        RemoveListener((Int32)evtType, handler);
    }
    public void DispatchEvent(GameEvent evt)
    {  
        handleEvent(evt);
    }
     
    public void DispatchEvent(int evtType, object parameter = null)
    {
        GameEvent GameEvent = CommonObjectPool<GameEvent>.Get();
        GameEvent.EventId = evtType;
        GameEvent.Data = parameter;
        DispatchEvent(GameEvent);
    }
    public void DispatchEvent(EventID evtType, object parameter = null)
    {
        DispatchEvent((int)evtType, parameter);
    }
    public void DispatchEvent(GuideStepEventId evtType, object parameter = null)
    {
        DispatchEvent((int)evtType, parameter);
    }

    private void handleEvent(GameEvent evt)
    {
        EventAction handler = getHandler(mEventHandlers, evt.EventId);
        if (handler != null)
        {
            handler(evt);
        }

        handler = getHandler(mEventOnceHandlers, evt.EventId);
        if (handler != null)
        {
            removeListener(mEventOnceHandlers, evt.EventId, handler);
            handler(evt);
        }
        evt.Recycle();
    }

    private void removeListener(Dictionary<Int32, EventAction> eventHandlers, Int32 evtType, EventAction handler)
    {
        EventAction exists = getHandler(eventHandlers, evtType);
        if (exists != null)
        {
            if (exists.GetInvocationList().Contains(handler))
            {
                exists -= handler;
                eventHandlers[evtType] = exists;
            }
            if (exists==null || exists.GetInvocationList().Length==0)
            {
                eventHandlers.Remove(evtType);
            } 
        }
    }

    private void addListener(Dictionary<Int32, EventAction> eventHandlers, Int32 evtType, EventAction handler)
    {
        EventAction exists = getHandler(eventHandlers, evtType);
        if (exists != null)
        {
            if (!exists.GetInvocationList().Contains(handler))
            {
                exists += handler; 
            }
            else
            {
                UnityEngine.Debug.LogError("多次添加事件:"+evtType.ToString());
            }
        }
        else
        {
            exists = handler;
        }
        eventHandlers[evtType] = exists; 
    }

    /// <summary>
    /// 返回handler
    /// </summary>
    /// <param name="eventHandlers"></param>
    /// <param name="evtType"></param>
    /// <returns></returns>
    private EventAction getHandler(Dictionary<Int32, EventAction> eventHandlers, Int32 evtType)
    {
        EventAction handler = null;
        eventHandlers.TryGetValue(evtType, out handler); 
        return handler;
    }

    /// <summary>
    /// 清除所有未分发的事件
    /// </summary>
    public void Clear()
    {
        mEventHandlers.Clear();
        mEventOnceHandlers.Clear();
    }
}

