using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
///场景资源加载
/// Date: 2016/1/20
/// Author: lxz  
/// </summary>
public class SceneAsset :BundleAsset
{
    string sceneName="";
    Action<float> onLoadUpdate;
    Action onAllComplete;

    string depConfResName="";
    public SceneAsset(AssetManager rmgr, string name)
        : base(rmgr, "s_" + name.ToLower() + IOTools.abSuffix)
    {
        sceneName = name;
        depConfResName = "s_" + name.ToLower() + "_dep" + IOTools.abSuffix;
    }

    /// <summary>
    /// 添加更新进度回调
    /// </summary>
    /// <param name="onUpdate"></param>
    public void AddLoadUpdateCall(Action<float> onUpdate)
    { 
        if (onLoadUpdate != null)
            onLoadUpdate += onUpdate;
        else
            onLoadUpdate = onUpdate;
    }

    /// <summary>
    /// 添加完成回调
    /// </summary>
    /// <param name="onComplete"></param>
    public void AddAllCompleteCall(Action onComplete)
    {
        if (onAllComplete != null)
            onAllComplete += onComplete;
        else
            onAllComplete = onComplete;
    }

    /// <summary>
    /// 异步加载场景内容
    /// </summary>
    protected  IEnumerator asyncLoadWillComplete()
    {
        AsyncOperation loadOpr = SceneManager.LoadSceneAsync(sceneName); 

        float process = 0.2f;
        while (!loadOpr.isDone)
        {
            process = 0.2f + loadOpr.progress * 0.267f;
            if (onLoadUpdate != null)
                onLoadUpdate(process);
            yield return null;
        }

        bundle.Unload(false);
        bundle = null;
         
        var root = GameObject.Find(sceneName);
        //根据性能配置 删除部分对象...暂时不写
        
        if (root != null)
        {
#if UNITY_EDITOR
            EditorHelper.SetEditorShader(root);
#endif
            var pfmh = root.GetComponent<PrefabMaterialHolder>();
            if (pfmh != null)
            {
                pfmh.isRemoveRefOnDestroy = true;
                //异步加载贴图
                yield return pfmh.AsyncLoadMatsTex(p =>
                {
                    process = 0.467f + p * 0.52f; 
                    if (onLoadUpdate != null)
                        onLoadUpdate(process);
                });
            } 
        }
        else
        {
            Debug.LogError("得到场景root失败;" + sceneName);
        }

        if (onLoadUpdate != null)
        {
            onLoadUpdate(1);
            onLoadUpdate = null;
        }
        if (onAllComplete != null)
        {
            onAllComplete();
            onAllComplete = null;
        } 
    }

    /// <summary>
    /// 场景默认不提供同步加载接口
    /// </summary>
    public override void Load()
    {
        Debug.LogError("scene res sync Load error!!"); 
    } 
     
    public IEnumerator asyncLoad()
    {
        if (state == ResLoadingState.LOADSTATE_LOADING)
            yield break;
        float process = 0.01f;
        if (onLoadUpdate != null)
            onLoadUpdate(process);
        yield return new WaitForEndOfFrame();
        base.Load();
        yield return CoroutineManager.Singleton.StartCoroutine(asyncLoadWillComplete());
    }
 
    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    { 
        if(state!= ResLoadingState.LOADSTATE_LOADED)
            return;
        base.Unload();    
        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
