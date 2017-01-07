using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary> 
///  特效资源管理类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>

public class EffectAssetManager : AssetManager
{
    static EffectAssetManager ins = null;
    public static EffectAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new EffectAssetManager(); 
                AssetManager.AddResourceMgr(ResourceType.Effect, ins);
            }
            return ins;
        }
    }

    protected override Asset getOrCreateResourceRefObj(string name)
    {
        var lname = name.ToLower();
        Asset res = null;
        if (!assets.TryGetValue(lname, out res))
        {
            res = new EffectAsset(this,lname);
            assets[lname] = res;
        }
        return res;
    } 
}
