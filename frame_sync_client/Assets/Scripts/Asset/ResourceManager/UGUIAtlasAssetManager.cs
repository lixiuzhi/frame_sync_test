using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using System;



/// <summary> 
/// ui图集资源管理类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>
public class UGUIAtlasAssetManager : AssetManager
{
    static UGUIAtlasAssetManager ins = null;
    public static UGUIAtlasAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new UGUIAtlasAssetManager();
                AssetManager.AddResourceMgr(ResourceType.UIAtlas, ins);
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
            res = new UGUIAtlasAsset(this, lname);
            assets[lname] = res;
        }
        return res;
    }
}
