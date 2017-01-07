using UnityEngine;
using System.Collections;
using System;

using System.IO;
using System.Text;


/// <summary>
/// 二进制和文本资源基类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>

public class BundleAsset: Asset
{
    public AssetBundle bundle = null;  

    public BundleAsset(AssetManager rmgr,string name):base(rmgr,name)
    {
    }  
 
    /// <summary>
    /// 同步加载ab
    /// </summary> 
    public static AssetBundle LoadAb(string name)
    {
        AssetBundle ab = null;
        if (IOTools.IsResInUpdateDir(name))
        { 
            string path = IOTools.getUpdateResPath(name);
            ab = AssetBundle.LoadFromFile(path); 
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            { 
                string path = IOTools.GetPackageResPath(name);
                ab = AssetBundle.LoadFromFile(path); 
            }
            else  //ios 判断目录，没有直接下载
            {
                string path = IOTools.GetPackageResPath(name);
                if (File.Exists(path))
                { 
                    ab = AssetBundle.LoadFromFile(path);
                }
            } 
        }

        if (ab==null)
        {
            Debug.LogError("Load bundle error：" + name); 
        } 
        return ab;
    }

    /// <summary>
    /// 异步加载一个ab
    /// 返回AssetBundleCreateRequest
    /// </summary> 
    protected static AssetBundleCreateRequest asyncLoadAb(string name)
    {
        AssetBundleCreateRequest abReq = null;
        if (IOTools.IsResInUpdateDir(name))
        {
            string path = IOTools.getUpdateResPath(name);
            abReq = AssetBundle.LoadFromFileAsync(path);
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                string path = IOTools.GetPackageResPath(name);
                abReq = AssetBundle.LoadFromFileAsync(path);
            }
            else  //ios 判断目录
            {
                string path = IOTools.GetPackageResPath(name);
                if (File.Exists(path))
                {
                    abReq = AssetBundle.LoadFromFileAsync(path);
                }
            }
        }
        return abReq; 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator asyncLoadReal()
    {
        if (state == ResLoadingState.LOADSTATE_UNLOADED)
        {
            state = ResLoadingState.LOADSTATE_LOADING;
            yield return asyncLoadAb(name); 
            refCount = 0;
            onBaseResLoadCompleteSyncCall();
            if (onCmp != null)
                onCmp.Call(this);
            state = ResLoadingState.LOADSTATE_LOADED;
        } 
        yield return null;
    }

    /// <summary>
    /// 同步加载ab
    /// </summary> 
    public override void Load()
    {  
        if (state == ResLoadingState.LOADSTATE_UNLOADED)
        { 
            bundle = LoadAb(name); 
            refCount = 0; 
            onBaseResLoadCompleteSyncCall();  
            if (onCmp != null)
                onCmp.Call(this);
            state = ResLoadingState.LOADSTATE_LOADED; 
            refCount = 0;
        } 
    }  

    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    {   
        if (bundle != null)
        {
            bundle.Unload(true);
            bundle = null;
        }
        state = ResLoadingState.LOADSTATE_UNLOADED;
        refCount = 0;
    }
}
