using UnityEngine;
using System;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.UI;


#if !USE_NOT_AB_UI_RES

/// <summary>
/// ui图集资源
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>

public class UGUIAtlasAsset : BundleAsset
{
    /// <summary>
    /// 保存图集名字和sprite对应关系
    /// </summary>
    Texture spriteTexture = null;
    Dictionary<string,Sprite> sprites = new Dictionary<string, Sprite>();

    /// <summary>
    /// 传入的名字必须是图集的名字，由内部来推导资源的名字
    /// </summary>
    /// <param name="rmgr">Rmgr.</param>
    /// <param name="name">Name.</param>
    public UGUIAtlasAsset(AssetManager rmgr,string name):base(rmgr,"uit_"+name.ToLower()+IOTools.abSuffix)
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Sprite GetSprite(string name)
    {
        Sprite sp = null;
        sprites.TryGetValue(name,out sp);
        return sp;
    }

    public Texture GetTexture()
    {
        return spriteTexture;
    }

    /// <summary>
    /// 加载图集sprite资源
    /// </summary>
    /// <param name="res">ResHelper.</param>
    protected override void onBaseResLoadCompleteSyncCall()
    {
        //加载sprites
        if (sprites.Count > 0)
        {
            Debug.LogError(">>>>>>>>sprite not Unload,Load again!");
            sprites.Clear();
            spriteTexture = null;
        }

        if (bundle != null)
        {
            var assets =  bundle.LoadAllAssets();
            for (int i = 0; i < assets.Length; i++)
            {
                Sprite sp = assets[i] as Sprite;
                if (sp != null)
                {
                    sprites[sp.name] = sp; 
                }
                else
                {
                    spriteTexture = assets[i] as Texture; 
                }
            }
            bundle.Unload(false);
        }
    }  

    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    {
        if (state != ResLoadingState.LOADSTATE_LOADED)
            return;

        base.Unload(); 
     
        foreach (var v in sprites)
        {
            if (v.Value != null)
            {
                GameObject.DestroyImmediate(v.Value,true);
            }
        }
        sprites.Clear();

        if (spriteTexture != null)
        {
            GameObject.DestroyImmediate(spriteTexture,true);
            spriteTexture = null;
        }
        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
#else 

using UnityEditor;
public class UIAtlasResource : Resource
{ 
    /// <summary>
    /// 保存图集名字和sprite对应关系
    /// </summary>
    Texture spriteTexture = null;
    Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    /// <summary>
    /// 传入的名字必须是图集的名字，由内部来推导资源的名字
    /// </summary>
    /// <param name="rmgr">Rmgr.</param>
    /// <param name="name">Name.</param>
    public UIAtlasResource(ResourceManager rmgr, string name)
        : base(rmgr, name)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Sprite getSprite(string name)
    {
        Sprite sp = null;
        sprites.TryGetValue(name, out sp);
        return sp;
    }

    public Texture getTexture()
    {
        return spriteTexture;
    }

    /// <summary>
    /// 加载图集sprite资源
    /// </summary>
    /// <param name="res">Res.</param>
    private void loadSprite(Resource res)
    {
        //加载sprites
        if (sprites.Count > 0)
        {
            Debug.LogError(">>>>>>>>sprite not unload,load again!");
            sprites.Clear();
            spriteTexture = null;
        }
    }

    public override bool load()
    {
        if (state != ResLoadingState.LOADSTATE_UNLOADED)
            return false;
        addCompleteTask(loadSprite, true);
        AssetDatabase.load
        return true;
    }

    /// <summary>
    /// 异步加载图集
    /// </summary>
    public override void load()
    {
        load();
    }



    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void unload()
    {
        if (state != ResLoadingState.LOADSTATE_LOADED)
            return;

        base.unload();

        foreach (var v in sprites)
        {
            if (v.Value != null)
            {
                GameObject.DestroyImmediate(v.Value, true);
            }
        }
        sprites.Clear();

        if (spriteTexture != null)
        {
            GameObject.DestroyImmediate(spriteTexture, true);
            spriteTexture = null;
        }
        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
#endif