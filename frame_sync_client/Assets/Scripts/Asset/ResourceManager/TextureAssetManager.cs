using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary> 
/// 公共纹理资源管理类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>
public class TextureAssetManager : AssetManager
{
    static TextureAssetManager ins = null;
    public static TextureAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new TextureAssetManager();
                AssetManager.AddResourceMgr(ResourceType.Texture, ins);
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
            res = new TextureAsset(this, lname);
            assets[lname] = res;
        }
        return res;
    }
}
