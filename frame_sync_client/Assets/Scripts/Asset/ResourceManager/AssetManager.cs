using UnityEngine;
using System.Collections;
using System;

using System.Collections.Generic;


public enum ResourceType
{
    None,
    UIAtlas,
    UIPage,
    Model,
    Audio,
    Effect,
    Texture,
    SceneObject,
    AnimationClip,
}

/// <summary> 
///子类根据需求可自定义缓存策略
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>
public class AssetManager
{
    #region 资源管理器集合
    static Hashtable resMgrMaps = new Hashtable();
    protected static void AddResourceMgr(ResourceType t, AssetManager resMgr)
    {
        resMgrMaps.Add(t,resMgr);
    }
    public static AssetManager getResourceMgr(ResourceType t)
    {
        return resMgrMaps[t] as AssetManager;
    }

    public static string ShowAllAsset()
    {
        string log = "";
        foreach (DictionaryEntry am in resMgrMaps)
        {
            var assets = ((AssetManager)am.Value).assets;
            foreach (var a in assets)
            {
                if (a.Value.ResState == ResLoadingState.LOADSTATE_LOADED)
                {
                    string str = am.Key + ":" + a.Value.name + " count:" + a.Value.ReferenceCount;
                    log += str+"\n";
                    Logger.err(str);
                }
            } 
        }
        return log;
    }

    public static void UnloadAllUnusedAsset()
    {
        foreach (DictionaryEntry am in resMgrMaps)
        {
            ((AssetManager)am.Value).UnloadUnreferencedResources();
        }
    }

    #endregion

    public delegate bool ResAsyncDelayCheckFunc();

    protected Dictionary<string, Asset> assets = new Dictionary<string,Asset>();

    protected virtual Asset getOrCreateResourceRefObj(string name)
    {
        return null;
    }

    public virtual Asset Load(string name)
    { 
        Asset res = getOrCreateResourceRefObj(name);
        if (res != null&&res.ResState== ResLoadingState.LOADSTATE_UNLOADED)
        { 
            res.Load(); 
        }
        return res;
    }

    public virtual Asset AsyncLoad(string name, Action<Asset,object> onLoadComplete,object data = null)
    {
        Asset res = getOrCreateResourceRefObj(name);
        if (res != null)
        {
            res.AddCompleteTask(onLoadComplete, data);
            res.AsyncLoad();
        }
        return res;
    }
    public virtual void addRef(string name)
    {
        var lname = name.ToLower();
        Asset res = null;
        assets.TryGetValue(lname, out res);
        if (res != null)
        {
            res.AddRef();
        }
    }

    public virtual void RemoveRef(string name)
    {
        var lname = name.ToLower();
        Asset res = null;
        assets.TryGetValue(lname, out res);
        if (res != null)
        {
            res.RemoveRef();
        }
    }

    public virtual void Unload(string name)
    {

    }
    public virtual void UnloadUnreferencedResources()
    {
        foreach(var v in assets)
        {
            if (v.Value.ReferenceCount <= 0)
            {
                v.Value.Unload();
            }
        }
    }
}
