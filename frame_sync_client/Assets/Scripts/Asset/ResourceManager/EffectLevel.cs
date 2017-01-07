using UnityEngine;
using System.Collections;

/// <summary> 
/// 特效等级类
/// 需要指明那些对象在中等和低等性能手机需要被销毁
/// Date: 2016/1/19
/// Author: lxz  
/// </summary>
public class EffectLevel : MonoBehaviour
{
    [Header("中等效果需删除对象")]
    public GameObject[] MiddleLevelHideObj;
    [Header("低等效果需删除对象")]
    [Header("也会隐藏中等效果对象，所以可不必把中等特效对象再拖到这里面")]
    public GameObject[] LowLevelHideObj;

    public float RecycleDelay = 1.0f;

    void Awake()
    {
        GameObject.Destroy(gameObject, RecycleDelay);
    }
}
