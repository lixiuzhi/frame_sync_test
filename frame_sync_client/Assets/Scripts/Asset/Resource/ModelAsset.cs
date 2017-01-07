using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ModelAsset :BundleAsset
{
    GameObject asset = null;
    PrefabMaterialHolder matHolder;
    AnimationHolder clipHolder;
    string modelName;
    public ModelAsset(AssetManager rmgr, string name)
        : base(rmgr, "m_" + name.ToLower() + IOTools.abSuffix)
    {
        modelName = name.ToLower();
    } 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject getAsset()
    { 
        return asset;
    }

    /// <summary>
    ///  
    /// </summary>
    /// <param name="res">ResHelper.</param>
    protected override void onBaseResLoadCompleteSyncCall()
    {
        if (bundle != null)
        {
            asset = bundle.LoadAsset<GameObject>(modelName);
            if (asset == null)
            {
                return;
            }
#if UNITY_EDITOR
            EditorHelper.SetEditorShader(asset);
#endif
            var refholder = asset.AddComponent<InastanceAssetRefHolder>();
            refholder.resType = ResourceType.Model;
            refholder.assetName = modelName;
            //加载模型贴图资源
            matHolder = asset.GetComponent<PrefabMaterialHolder>();
            if (matHolder != null)
            {
                matHolder.SyncLoadMatsTex();
            }
            //加载动作clip资源
            clipHolder = asset.GetComponent<AnimationHolder>();
            if (clipHolder != null)
            {
                clipHolder.SyncLoadClips();
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
            {
                if (onCmp != null)
                    onCmp.Clear();
                Logger.err("async load bundle error:" + name); 
                state = ResLoadingState.LOADSTATE_UNLOADED;
                yield break;
            }
            yield return req;
              
            bundle = req.assetBundle;
            refCount = 0;
            #region 异步加载贴图等资源，设置模型参数
            if (bundle != null)
            {
                //异步加载bundle里的资源
                var assetReq = bundle.LoadAssetAsync<GameObject>(modelName);
                yield return assetReq;
                asset = assetReq.asset as GameObject;
                if (asset == null)
                {
                    state = ResLoadingState.LOADSTATE_UNLOADED;
                    yield break;
                }
                state = ResLoadingState.LOADSTATE_LOADING;
#if UNITY_EDITOR
                EditorHelper.SetEditorShader(asset);
#endif 
                //异步加载模型贴图资源
                var refholder = asset.AddComponent<InastanceAssetRefHolder>();
                refholder.resType = ResourceType.Model;
                refholder.assetName = modelName;
                matHolder = asset.GetComponent<PrefabMaterialHolder>();
                if (matHolder != null)
                {
                    yield return matHolder.AsyncLoadMatsTex();
                }

                //异步加载动作clip资源
                clipHolder = asset.GetComponent<AnimationHolder>();
                if (clipHolder != null)
                {
                    yield return clipHolder.AsyncLoadClips();
                }

                #endregion
                if (onCmp != null)
                    onCmp.Call(this);
                state = ResLoadingState.LOADSTATE_LOADED;
            }
            else
            {
                if (onCmp != null)
                    onCmp.Clear();
                Logger.err("async load bundle error:"+name);
                state = ResLoadingState.LOADSTATE_UNLOADED;
            }
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

        //移除公共贴图引用
        if (matHolder != null)
        {
            matHolder.removeTexturesRef();
        }
        //移除clip引用
        if (clipHolder != null)
        {
            clipHolder.removeClipsRef();
        }

        GameObject.DestroyImmediate(asset,true);
        asset = null;  
        if (bundle != null)
        {
            matHolder = null; 
            bundle.Unload(true);
        } 
        base.Unload();  
    }
}
