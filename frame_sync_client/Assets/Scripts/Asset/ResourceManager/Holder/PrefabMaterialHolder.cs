using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>
/// 保存预制件打包时候去掉的贴图依赖信息
/// Date: 2016/1/19
/// Author: lxz  
/// </summary>
public class PrefabMaterialHolder : MonoBehaviour
{ 
    [System.Serializable]
    public class MaterialTextureInfo
    {
        public string attribute;
        public string tex2dName;
    }

    [System.Serializable]
    public class MaterialAllTextureInfo
    {
        public int matIndex;
        public MaterialTextureInfo[] matTexInfos;
    }

    [System.Serializable]
    public class RenderMatTexPair
    {
        public Renderer renderObj;
        public MaterialAllTextureInfo[] matAllInfos;
    }

    public List<RenderMatTexPair> rendersMatTexInfo;

    /// <summary>
    /// 保存引用的贴图资源,释放的时候好处理
    /// </summary>
    List<Asset> texResRefs = new List<Asset>();

    /// <summary>
    /// 是否在ondestroy函数调用的时候 移除贴图引用
    /// 主要针对场景资源贴图的释放
    /// </summary> 
    public bool isRemoveRefOnDestroy = false;
     
    /// <summary>
    /// 同步加载用到的所有贴图
    /// </summary>
    public void SyncLoadMatsTex()
    {
        if (rendersMatTexInfo == null)
            return;
        int len = rendersMatTexInfo.Count;
        for (int i = 0; i < len; i++)
        {
            if (rendersMatTexInfo[i].renderObj == null)
            {
                continue;
            }
            var renderMatInfos = rendersMatTexInfo[i].matAllInfos;
            if (renderMatInfos != null)
            {
                if (renderMatInfos.Length == 1)
                {
                    var matTexInfos = renderMatInfos[0].matTexInfos;
                    for (int m = 0; m < matTexInfos.Length; m++)
                    {
                        var mat = rendersMatTexInfo[i].renderObj.sharedMaterial;
                        var attName = matTexInfos[m].attribute;
                        TextureAsset res = (TextureAsset)TextureAssetManager.Singleton.Load(matTexInfos[m].tex2dName);
                        var tex = ((TextureAsset)res).getAsset();
                        if (tex != null)
                        {
                            mat.SetTexture(attName, tex);
                            res.AddRef();
                            //记录加载了哪些贴图
                            texResRefs.Add(res);
                        }
                    }
                }
            }
            else
            {
                var mats = rendersMatTexInfo[i].renderObj.sharedMaterials;
                for (int j = 0; j < renderMatInfos.Length; j++)
                {
                    if (mats.Length > renderMatInfos[j].matIndex && mats[j] != null)
                    {
                        var mtis = renderMatInfos[j].matTexInfos;
                        for (int n = 0; n < mtis.Length; n++)
                        { 
                            var mat = mats[renderMatInfos[j].matIndex];
                            var attName = mtis[n].attribute; 
                            TextureAsset res = (TextureAsset)TextureAssetManager.Singleton.Load(mtis[n].tex2dName);
                            mat.SetTexture(attName, ((TextureAsset)res).getAsset());
                            res.AddRef(); 
                        }
                    }
                }
                rendersMatTexInfo[i].renderObj.sharedMaterials = mats;
            }
        }
    }

    /// <summary>
    /// 贴图异步加载
    /// </summary> 
    public IEnumerator AsyncLoadMatsTex(Action<float> OnProcessUpdate=null)
    {
        if (rendersMatTexInfo == null)
            yield break;
        int len = rendersMatTexInfo.Count;

        int asyncStep = Mathf.Max(1,len / 15); 

        for (int i = 0; i < len; i++)
        {
            if (rendersMatTexInfo[i].renderObj == null)
            {
                continue;
            }
            var renderMatInfos = rendersMatTexInfo[i].matAllInfos;
            if (renderMatInfos != null)
            {
                if (renderMatInfos.Length == 1)
                {
                    var matTexInfos = renderMatInfos[0].matTexInfos;
                    for (int m = 0; m < matTexInfos.Length; m++)
                    {
                        var mat = rendersMatTexInfo[i].renderObj.sharedMaterial;
                        var attName = matTexInfos[m].attribute;
                        var res = TextureAssetManager.Singleton.Load(matTexInfos[m].tex2dName);
                        var tex = ((TextureAsset)res).getAsset();
                        if (tex != null)
                        {
                            mat.SetTexture(attName, tex);
                            res.AddRef();
                            //记录加载了哪些贴图
                            texResRefs.Add(res);
                        }
                        if (i % asyncStep == 0)
                            yield return null;
                    }
                }
                else
                {
                    var mats = rendersMatTexInfo[i].renderObj.sharedMaterials;
                    for (int j = 0; j < renderMatInfos.Length; j++)
                    {
                        if (mats.Length > renderMatInfos[j].matIndex && mats[j] != null)
                        {
                            var mtis = renderMatInfos[j].matTexInfos;
                            for (int n = 0; n < mtis.Length; n++)
                            { 
                                var mat = mats[renderMatInfos[j].matIndex];
                                var attName = mtis[n].attribute;
                                var res = TextureAssetManager.Singleton.Load(mtis[n].tex2dName);
                                var tex = ((TextureAsset)res).getAsset();
                                if (tex != null)
                                {
                                    mat.SetTexture(attName, tex);
                                    res.AddRef();
                                    texResRefs.Add(res);
                                }
                                if (i % asyncStep == 0)
                                    yield return null;
                            }
                        }
                    }
                    rendersMatTexInfo[i].renderObj.sharedMaterials = mats;
                }
            }
            if (i % asyncStep == 0)
                yield return null;
            if (OnProcessUpdate != null)
            {
                OnProcessUpdate(i * 1.0f / len);
            }
        }
        if (OnProcessUpdate != null)
        {
            OnProcessUpdate(1f);
        }
    }

    /// <summary>
    /// 卸载资源的时候，移除对贴图的引用
    /// </summary>
    public void removeTexturesRef()
    {
        for (int i = 0; i < texResRefs.Count; i++)
        {
            if (texResRefs[i] != null)
            {
                texResRefs[i].RemoveRef();
            }
        }
        texResRefs.Clear();
    }

    void OnDestroy()
    {
        if (isRemoveRefOnDestroy)
        {
            removeTexturesRef();
        }
    }
}
