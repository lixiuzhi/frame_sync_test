using UnityEngine;
using System.Collections;

/// <summary>
/// 对于实例化的资源进行引用增减
/// </summary>
public class InastanceAssetRefHolder : MonoBehaviour
{
    //在ab资源加载完成后，在资源上面挂在这个脚本，并初始化下面变量，资源不会执行函数，但实例化的对象会
    public ResourceType resType = ResourceType.None;
    public string assetName = string.Empty;
    AssetManager resMgr;

    void Awake()
    {
        resMgr = AssetManager.getResourceMgr(resType);
        if (resMgr != null)
        {
            resMgr.addRef(assetName);
        }
    }

    void OnDestroy()
    {
        if (resMgr != null)
        {
            resMgr.RemoveRef(assetName);
        }
    }
}
