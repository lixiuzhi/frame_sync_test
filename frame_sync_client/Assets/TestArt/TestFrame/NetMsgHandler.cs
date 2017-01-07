 
using System; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LuaInterface;

public delegate void MsgAction(MessageData msg);
public class NetMsgHandler : SingletonTemplate<NetMsgHandler>
{ 
    private Dictionary<Int32, MsgAction>  handlers; 

    /// <summary>
    /// 构造
    /// </summary> 
    public NetMsgHandler()
    {
        handlers = new Dictionary<Int32, MsgAction>();  
    }

    /// <summary>
    /// 添加一个事件监听
    /// </summary>
    /// <param name="evtType">监听的事件类型</param>
    /// <param name="handler">回调处理</param> 
    public void AddListener(Int32 msgID, MsgAction handler)
    {
        addListener(handlers, msgID, handler);
    }
  
 
    /// <summary>
    /// 分派事件
    /// </summary>
    /// <param name="evt"></param> 
    public void DispatchEvent(MessageData evt)
    { 
        handleEvent(evt);
    }

    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="evt"></param>  
    private void handleEvent(MessageData evt)
    {
        MsgAction handler = getHandler(handlers, evt.msgID);
        if (handler != null)
        {
            handler(evt);
        }
        else
        {
            UnityEngine.Debug.LogError("没有handle:" + evt.msgID);
        }
    }
    

    /// <summary>
    /// 添加事件处理实现
    /// </summary>
    /// <param name="eventHandlers"></param>
    /// <param name="evtType"></param>
    /// <param name="handler"></param>
    private void addListener(Dictionary<Int32, MsgAction> eventHandlers, Int32 evtType, MsgAction handler)
    {
        MsgAction exists = getHandler(eventHandlers, evtType);
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
    private MsgAction getHandler(Dictionary<Int32, MsgAction> eventHandlers, Int32 evtType)
    {
        MsgAction handler = null;
        eventHandlers.TryGetValue(evtType, out handler); 
        return handler;
    }
     
}

