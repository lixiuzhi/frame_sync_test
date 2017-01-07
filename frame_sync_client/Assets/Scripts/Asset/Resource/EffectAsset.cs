using UnityEngine;
using System.Collections;


/// <summary>
/// 特效资源，加载特效预制件ab同时，会加载贴图资源
/// Date: 2016/1/19
/// Author: lxz  
/// </summary>
public class EffectAsset :BundleAsset
{
      /// <summary>
    /// 保存图集名字和sprite对应关系
    /// </summary>
    GameObject asset = null;
    PrefabMaterialHolder matHolder;
    string effectName;

    /// <summary>
    /// 传入的名字必须是图集的名字，由内部来推导资源的名字
    /// </summary> 
    public EffectAsset(AssetManager rmgr, string name)
        : base(rmgr, "eft_" + name.ToLower() + IOTools.abSuffix)
    {
        effectName = name;
    }
        
    public GameObject getAsset()
    {
        return asset;
    } 

    /// <summary>
    /// 加载贴图
    /// </summary>
    /// <param name="res">ResHelper.</param>
    protected override void onBaseResLoadCompleteSyncCall()
    {
        if (asset != null)
        {
            Debug.LogError("effect asset need Unload:" + effectName);
        }
        if (bundle != null)
        {
            asset = bundle.LoadAsset<GameObject>(effectName); 
          
            if (asset != null)
            {  
#if UNITY_EDITOR
                EditorHelper.SetEditorShader(asset);
#endif

                var insHolder = asset.AddComponent<InastanceAssetRefHolder>();
                insHolder.resType = ResourceType.Effect;
                insHolder.assetName = effectName;

                //针对设备性能，设置特效参数，删除一些配置 判断当前特效等级。如果中低特效，需要删除对应的对象，再加载对应的图片资源
                //TODO.... 

                //加载纹理资源
                matHolder = asset.GetComponent<PrefabMaterialHolder>();
                if (matHolder != null)
                {
                    matHolder.SyncLoadMatsTex();
                }
            }
        } 
    }


    /// <summary>
    /// 真正异步加载部分重写
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator asyncLoadReal()
    {
        if (state == ResLoadingState.LOADSTATE_UNLOADED)
        {
            state = ResLoadingState.LOADSTATE_LOADING;
            var req = asyncLoadAb(name);
            if (req == null)
                yield break;
            yield return req;
             
            bundle = req.assetBundle;
           refCount = 0;
            #region 异步加载贴图等资源，设置模型参数
            if (bundle != null)
            {
                //异步加载bundle里的资源
                asset = bundle.LoadAsset<GameObject>(effectName); 
                if (asset == null)
                {
                    state = ResLoadingState.LOADSTATE_UNLOADED;
                    yield break;
                }

                //针对设备性能，设置特效参数，删除一些配置 判断当前特效等级。如果中低特效，需要删除对应的对象，再加载对应的图片资源
                //TODO.... 

                state = ResLoadingState.LOADSTATE_LOADING;
#if UNITY_EDITOR
                EditorHelper.SetEditorShader(asset);
#endif 
                //异步加载模型贴图资源
                var refholder = asset.AddComponent<InastanceAssetRefHolder>();
                refholder.resType = ResourceType.Effect;
                refholder.assetName = effectName;
                matHolder = asset.GetComponent<PrefabMaterialHolder>();
                if (matHolder != null)
                {
                    yield return matHolder.AsyncLoadMatsTex();
                }
            }
            #endregion
            if (onCmp != null)
                onCmp.Call(this);
            state = ResLoadingState.LOADSTATE_LOADED;
        }
        else
        {
            if (onCmp != null)
                onCmp.Call(this);
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

        //销毁公共贴图引用
        if (matHolder != null)
        {
            matHolder.removeTexturesRef(); 
            matHolder = null;
        } 
        asset = null; 

        if (bundle != null)
        { 
            bundle.Unload(true);
        }
        base.Unload();    
    }
}
