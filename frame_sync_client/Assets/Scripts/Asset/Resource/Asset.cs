using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;

using LuaInterface;


/// <summary>
/// 资源基类，所有资源磊都应该继承此类
/// 提供基本接口，同步异步加载接口，加载完成回调接口
/// Date: 2016/1/4
/// Author: lxz  
/// </summary> 
public enum ResLoadingState
{
    ///已经卸载
    LOADSTATE_UNLOADED,
    ///加载中
    LOADSTATE_LOADING,
    ///加载完成
    LOADSTATE_LOADED,    
};
 
public class LoadCompleteTask
{
    class CSActionPair
    {
        public Action<Asset, object> f;
        public object data;
    } 

    private List<CSActionPair> funcs = null;
 

    public void AddTask(Action<Asset, object> f, object data = null)
    {
        if (funcs == null)
            funcs = new List<CSActionPair>();
        funcs.Add(new CSActionPair() { f = f, data = data });
    }
 

    public void Call(Asset res)
    {
        if (funcs != null)
        {
            for (int i = 0; i < funcs.Count; i++)
            { 
                funcs[i].f(res, funcs[i].data);
            }
        }    

        Clear();
    }

     public void Clear()
    {
        if (funcs != null)
        {
            funcs.Clear();
        } 
    }
}

public abstract class Asset
{    
    protected LoadCompleteTask onCmp = null;

    protected ResLoadingState state = ResLoadingState.LOADSTATE_UNLOADED;
    protected AssetManager creator;
    public string name;
    protected long refCount = 0;

    Action onLoadError = null;

    public long ReferenceCount
    {
        get 
        {
            return refCount;
        }
    }

    /// <summary>
    /// 当前资源状态
    /// </summary>
    public ResLoadingState ResState
    {
        get 
        {
            return state;
        }
        set
        {
            state = value;
        }
    }

    public Asset(AssetManager resMgr,string name)
    {
        this.creator = resMgr;
        this.name = name;
    }

    /// <summary>
    /// 加载完成的时候首先调用
    /// </summary>
    protected virtual void onBaseResLoadCompleteSyncCall()
    {

    }

    /// <summary>
    /// 同步加载
    /// </summary>
    public abstract void Load();

    public void AsyncLoad()
    {
        if (state == ResLoadingState.LOADSTATE_UNLOADED)
        {
            CoroutineManager.Singleton.AddCoroutine(asyncLoadReal());
        }
        else if(state== ResLoadingState.LOADSTATE_LOADED)
        {
            if (onCmp != null)
                onCmp.Call(this);
        }
    }

    /// <summary>
    /// 真正的异步加载
    /// </summary>
    protected virtual IEnumerator asyncLoadReal()
    {
        yield return null;
    }


    public void AddCompleteTask(Action<Asset,object> f,object data = null)
    {
        if (state== ResLoadingState.LOADSTATE_LOADED)
        {
            f(this,data);
            return;
        }
        if (onCmp == null)
            onCmp = new LoadCompleteTask();
        onCmp.AddTask(f,data);
    }


    public void AddLoadErrorTask(Action action)
    {
        if (onLoadError == null)
            onLoadError = action;
        else
        {
            onLoadError += action;
        }
    } 

    /// <summary>
    /// 卸载资源 
    /// </summary>
    public abstract void Unload();
     
    public AssetManager GetCreator()
    {
        return creator;
    }

    public void AddRef()
    { 
        if (refCount <= 0)
        {
            refCount = 0;
        }
        refCount++; 
    }

    public void RemoveRef()
    {
        refCount--;
        if (refCount <= 0)
        {
            refCount = 0;
        }
    }
}
