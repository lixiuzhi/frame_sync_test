using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary> 
/// 模型资源管理类
/// Date: 2016/1/25
/// Author: lxz  
/// </summary> 
public class ModelAssetManager : AssetManager
{
    static ModelAssetManager ins = null;
    public static ModelAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new ModelAssetManager(); 
                AssetManager.AddResourceMgr(ResourceType.Model, ins);
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
            assets[lname] =(res = new ModelAsset(this, lname));
        }
        return res;
    }
}
