using UnityEngine;
using System.Collections;
 
/// <summary>
/// 音效资源类
/// Date: 2016/1/21
/// Author: lxz  
/// </summary>
public class AudioAsset :BundleAsset
{
    AudioClip ac=null;
    public AudioAsset(AssetManager rmgr, string name)
        : base(rmgr, "snd_" + name.ToLower() + IOTools.abSuffix)
    { 
    }

    public AudioClip GetAsset()
    {
        return ac;
    }

    /// <summary>
    /// 加载图集sprite资源
    /// </summary> 
    protected override void onBaseResLoadCompleteSyncCall()
    {
        if (ac != null)
        {
            Debug.LogError("audio clip need Unload:" + name);
        }
        if (bundle != null)
        {
            var clip = bundle.LoadAllAssets();
            if (clip == null)
            {
                return;
            }
            ac = clip[0] as AudioClip;
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

        if (ac != null)
        {
            GameObject.DestroyImmediate(ac, true);
            ac = null;
        }
        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
