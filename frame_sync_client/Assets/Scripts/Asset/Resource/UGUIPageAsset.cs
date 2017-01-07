using UnityEngine;
using System;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.UI;
 
/// <summary>
/// ui预制件ab资源
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>

public class UGUIPageAsset : BundleAsset
{
    /// <summary>
    /// 传入的名字必须是图集的名字，由内部来推导资源的名字
    /// </summary>
    /// <param name="rmgr">Rmgr.</param>
    /// <param name="name">Name.</param>
    public UGUIPageAsset(AssetManager rmgr, string name)
        : base(rmgr, "uiw_" + name + IOTools.abSuffix)
    {
        winName = name;
    }
    string winName = "";
    GameObject asset = null;
    GuiHolder guiHodler;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject getAsset()
    { 
        return asset;
    }

    protected override void onBaseResLoadCompleteSyncCall()
    { 
        //异步加载所有图集资源
        if (bundle != null)
        {
            asset = bundle.LoadAsset<GameObject>(winName);
            if (asset == null)
            {
                bundle.Unload(true);
              //  comTask.Clear();
                return;
            }
#if UNITY_EDITOR
            EditorHelper.SetEditorShader(asset);
#endif
            var refholder = asset.AddComponent<InastanceAssetRefHolder>();
            refholder.resType = ResourceType.UIPage;
            refholder.assetName = winName;
            //加载窗口image贴图
            guiHodler = asset.GetComponent<GuiHolder>();
            if (guiHodler != null)
            {
                guiHodler.LoadUIAtlas(); 
            }
            else
            {
                Debug.LogWarning(">>>>窗口资源没有找到GuiHolder脚本!!!" + winName);
            }
        }
    } 
     
    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    {
        if (state != ResLoadingState.LOADSTATE_LOADED)
            return;
        if (guiHodler != null)
        {
            guiHodler.RemoveUIAtlassRef();
        }
        if (asset != null)
        {
            GameObject.DestroyImmediate(asset,true);
        }

        if (bundle != null)
        {
            bundle.Unload(true);
        }
        guiHodler = null;
        asset = null;
        bundle = null;

        base.Unload();  
    }
}
 