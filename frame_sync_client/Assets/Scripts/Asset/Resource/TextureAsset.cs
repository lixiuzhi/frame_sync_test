using UnityEngine;
using System.Collections;

//根据不同类型，根据机型确定加载特效贴图资源
public enum EffectType
{
    UI,
    SCENE,
    SKILL,
    CHARACTOR,
    MODEL,
}

/// <summary>
/// 公共贴图资源类
/// Date: 2016/1/18
/// Author: lxz  
/// </summary>
public class TextureAsset :BundleAsset
{
      /// <summary>
    /// 保存图集名字和sprite对应关系
    /// </summary>
    Texture texture = null; 
    string texturName;
    /// <summary>
    /// 传入的名字必须是图集的名字，由内部来推导资源的名字
    /// </summary>
    /// <param name="rmgr">Rmgr.</param>
    /// <param name="name">Name.</param>
    public TextureAsset(AssetManager rmgr,string name):base(rmgr,"st_"+name.ToLower()+IOTools.abSuffix)
    { 
        texturName = name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Texture getAsset()
    { 
        return texture;
    } 

    /// <summary>
    /// 加载图集sprite资源
    /// </summary>
    /// <param name="res">ResHelper.</param>
    protected override void onBaseResLoadCompleteSyncCall()
    { 
        if(texture!=null)
        {
            Debug.LogError("share texture need Unload:"+name);
        }
        if (bundle != null)
        {
            texture = bundle.LoadAsset<Texture>(texturName);
            bundle.Unload(false);
        }
    } 
     
    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    { 
        if(state!= ResLoadingState.LOADSTATE_LOADED)
            return;
        base.Unload();   

        if(texture!=null)
        {
            GameObject.DestroyImmediate(texture,true);
            texture = null;
        }
      
        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
