using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AnimationHolder : MonoBehaviour {

        /// <summary>
        /// 如果 没有动画组件 为null
        /// </summary>
        public Animation ani;
        /// <summary>
        /// 动作文件全部分离
        /// </summary>
        public List<string> depAnimList;
        /// <summary>
        /// 默认动画
        /// </summary>
        public string defaultAnim;

    public void SyncLoadClips()
    {
        if (ani == null || depAnimList==null)
            return;
        
        int len = depAnimList.Count;
        var clipMgr = AnimationClipAssetManager.Singleton;
        for (int i = 0; i < len; i++)
        {
            var abName = depAnimList[i];
            var asset = clipMgr.Load(abName); 
            var clip = (asset as AnimationClipAsset).GetAsset();
            if (clip != null)
            {
                ani.AddClip(clip,clip.name);
                if (defaultAnim == abName)
                    ani.clip = clip;

                asset.AddRef();
            }
        }
    }

    /// <summary>
    /// 贴图异步加载
    /// </summary> 
    public IEnumerator AsyncLoadClips()
    {
        if (ani == null || depAnimList == null)
            yield break;

        int len = depAnimList.Count;
        var clipMgr = AnimationClipAssetManager.Singleton;
        for (int i = 0; i < len; i++)
        {
            var abName = depAnimList[i];
            var asset = clipMgr.Load(abName);
            var clip = (asset as AnimationClipAsset).GetAsset();
            if (clip != null)
            {
                ani.AddClip(clip, clip.name);
                if (defaultAnim == abName)
                    ani.clip = clip;
            }
            asset.AddRef();
            yield return null;
        }
    }

    public void removeClipsRef()
    {
        var clipMgr = AnimationClipAssetManager.Singleton;

        for (int i = 0; i < depAnimList.Count; i++)
        {
            clipMgr.RemoveRef(depAnimList[i]);
        } 
    }
}
