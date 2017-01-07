using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System;


/// <summary> 
/// ui页面资源管理类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>
 
public class UGUIPageAssetManager : AssetManager
{
    static UGUIPageAssetManager ins;
    public static UGUIPageAssetManager Singleton
    {
        get 
        {
            if (ins == null)
            {
                ins = new UGUIPageAssetManager();
                AssetManager.AddResourceMgr(ResourceType.UIPage, ins);
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
            res = new UGUIPageAsset(this, lname);
            assets[lname] = res;
        }
        return res;
    }
}
