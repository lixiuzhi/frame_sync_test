using UnityEngine;
using System.Collections;

//用于异步时序同步
public class UIImageHolder : MonoBehaviour
{ 
    public int seqValue=0;
    public Asset resourceRef;
    bool isInit = false;

    /// <summary>
    /// awake执行时机，针对在使用过程中，从之前的image对象生成新的对象，而没有手动添加引用的情况
    /// 防止过程中new的新image对象
    /// </summary>
    void Awake()
    {
        if (resourceRef != null)
        {
            resourceRef.AddRef();
        }
    }

    void OnDestroy()
    {
        if (resourceRef != null)
        {
            resourceRef.RemoveRef();
            resourceRef = null;
        }
    }
}
