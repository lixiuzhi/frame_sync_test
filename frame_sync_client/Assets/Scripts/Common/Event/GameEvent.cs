
using System;


/// <summary>
/// 游戏事件类
/// </summary> 
public class GameEvent : IRecycleObject
{ 
    /// <summary>
    /// 事件类型
    /// </summary>
    public Int32 EventId { get; set; }

    /// <summary>
    /// 事件参数
    /// </summary>
    public object Data { get; set; }
     
    public void ReAlloc()
    {
        EventId = -1;
        Data = null;
    }

    /// <summary>
    /// 实现回收接口
    /// </summary>
    public void Recycle()
    {
        EventId = -1;
        var dge = Data as GameEvent;
        if (Data != null && dge != null && dge.Data!=this)
        {
            dge.Recycle();
        }
        Data = null; 
    }
}

