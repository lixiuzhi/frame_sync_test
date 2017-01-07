using UnityEngine;
using System.Collections;

public class AnimationClipAsset : BundleAsset {

    AnimationClip clip = null;
    public AnimationClipAsset(AssetManager rmgr, string name)
        : base(rmgr,name)
    {
    }

    public AnimationClip GetAsset()
    {
        return clip;
    }

    protected override void onBaseResLoadCompleteSyncCall()
    {
        if (clip!=null)
        {
            Debug.LogError("animation clip need Unload:" + name);
        }
        if (bundle != null)
        {
            clip = bundle.LoadAllAssets()[0] as AnimationClip;
            if (clip == null)
            {
                Debug.LogError("load clip error:"+name);
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

        if (clip != null)
        {
            GameObject.DestroyImmediate(clip, true);
            clip = null;
        }
        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
