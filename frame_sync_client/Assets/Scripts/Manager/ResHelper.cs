using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;  



public static class ResHelper
{
    public delegate bool LoadCompleteCheck();
    /// <summary>
    /// 同步加载模型，返回prefab对象
    /// </summary> 
     public static GameObject LoadModel(string name)
     {
         var res = ModelAssetManager.Singleton.Load(name);
         return ((ModelAsset)res).getAsset();
     }

    /// <summary>
    /// 异步加载模型
    /// </summary> 
    /// <param name="onLoadComplete">加载完成之后的回调</param>
    /// <param name="check">调用完成回调之前判断是否满足条件</param>
    public static void AsyncLoadModel(string name,Action<Asset,object> onLoadComplete,object data = null)
    { 
        var res = ModelAssetManager.Singleton.AsyncLoad(name,onLoadComplete,data); 
    }

    /// <summary>
    /// 同步加载特效,返回prefab对象
    /// </summary> 
    public static GameObject LoadEffect(string name)
     { 
         var res = EffectAssetManager.Singleton.Load(name);
         return ((EffectAsset)res).getAsset();
     }


    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="name"></param>
    public static void PlaySound(string name)
    {
        AudioAssetManager.Singleton.PlaySound(name);
    }

    /// <summary>
    /// 播放全局背景音乐
    /// </summary>
    public static void PlayBGM(string name)
    {
        AudioAssetManager.Singleton.PlayBGM(name); 
    }

    #region UGUI资源相关
    /*
     /// <summary>
     /// 同步加载窗口
     /// </summary> 

     public static GameObject LoadUIWindow(string name)
     { 
         var res = UGUIPageAssetManager.Singleton.Load(name);
         return ((UGUIPageAsset)res).getAsset();
     }

    /// <summary>
    /// 动态设置ui image sprite 
    /// </summary>
     public static void SetImageSprite(Image image, string uiatlas, string uisprite)
     {
         if (image == null)
             return;
         UIImageHolder holder = null;
         holder = image.GetComponent<UIImageHolder>();
         if (holder == null)
         {
             holder = image.gameObject.AddComponent<UIImageHolder>();
         }
         //holder.seqValue++; 异步时序控制，改成同步后，可先不管
         int oldSeqValue = holder.seqValue;
         UGUIAtlasAsset res = (UGUIAtlasAsset)UGUIAtlasAssetManager.Singleton.Load(uiatlas);

         if (image != null && oldSeqValue == holder.seqValue)
         {
             if (image.sprite != null)
             {
                 if (holder.resourceRef != null)
                 {
                     holder.resourceRef.RemoveRef();
                     holder.resourceRef = null;
                 }
             }
             var sprite = res.GetSprite(uisprite);
             if (sprite != null)
             {
                 res.AddRef();
                 holder.resourceRef = res;
                 image.sprite = sprite;
             }
         }
     }

    /// <summary>
    /// 设置ui image sprite为空
    /// </summary>
    public static void SetImageSpriteNull(Image image)
    {
        if (image!=null&&image.sprite != null)
        {
            var holder = image.GetComponent<UIImageHolder>();
            if (holder == null)
            {
                holder = image.gameObject.AddComponent<UIImageHolder>();
            }

            if (holder.resourceRef != null)
            {
                holder.resourceRef.RemoveRef();
                holder.resourceRef = null;
            } 

            image.sprite = null; 
        }
    }
    */
    #endregion

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="obj"></param>
    public static void Destroy(UnityEngine.Object obj,ResourceType type) 
    {
        if (obj == null)
            return;
        switch (type)
        {
            case ResourceType.Audio://自动管理
                break;
            case ResourceType.Effect://自动管理
                if (obj != null)
                    GameObject.Destroy(obj);
                break;
            case ResourceType.Model: //自动管理 
                if (obj != null)
                    GameObject.Destroy(obj);
                break; 
            case ResourceType.Texture:
                break;
          /*case ResourceType.UIAtlas://自动管理  ugui相关
                break;
            case ResourceType.UIPage:
                break;
          */
            default:
                if (obj!=null)
                GameObject.Destroy(obj);
                break;
        } 
    }
}