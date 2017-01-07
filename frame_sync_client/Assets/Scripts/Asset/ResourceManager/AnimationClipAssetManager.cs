using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary> 
/// 模型资源管理类
/// Date: 2016/1/25
/// Author: lxz  
/// </summary> 
public class AnimationClipAssetManager: AssetManager
{
    static AnimationClipAssetManager ins = null;
    public static AnimationClipAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new AnimationClipAssetManager(); 
                AssetManager.AddResourceMgr(ResourceType.AnimationClip, ins);
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
            assets[lname] =(res = new AnimationClipAsset(this, lname));
        }
        return res;
    }
}
