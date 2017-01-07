using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

using System.Collections;

 
public class GuiHolder : MonoBehaviour
{
    [Serializable]
    public class ImageDic
    {
        public Image img;
        public string textureName;
        public string spriteName;
    } 

    public List<ImageDic> depAtlass;
    List<Asset> uiAtlassRef = new List<Asset>();

    public void LoadUIAtlas()
    {
        if (depAtlass != null)
        {
            int count = depAtlass.Count;
            for (int i = 0; i < count; i++)
            {
                if (depAtlass[i].img != null && depAtlass[i].img.sprite == null)
                {
                    UGUIAtlasAsset res = (UGUIAtlasAsset)UGUIAtlasAssetManager.Singleton.Load(depAtlass[i].textureName);

                    if (this != null && depAtlass[i] != null && depAtlass[i].img != null)
                    {
                        uiAtlassRef.Add(res);
                        depAtlass[i].img.sprite = res.GetSprite(depAtlass[i].spriteName);
                        var seq = depAtlass[i].img.gameObject.AddComponent<UIImageHolder>();
                        res.AddRef();
                        seq.seqValue = 0;
                        seq.resourceRef = res;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 卸载资源的时候，移除对UI图集的引用
    /// </summary>
    public void RemoveUIAtlassRef()
    {
        for (int i = 0; i < uiAtlassRef.Count; i++)
        {
            if (uiAtlassRef[i] != null)
            {
                uiAtlassRef[i].RemoveRef();
            }
        }
        uiAtlassRef.Clear();
    }
}

