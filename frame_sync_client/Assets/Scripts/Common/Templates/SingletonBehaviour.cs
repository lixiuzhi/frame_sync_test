/*
 * file SingletonBehaviour.cs
 *
 * author: 
 * date:   2014/09/16 
 */

using UnityEngine;


/// <summary>
/// 单件模版 
/// 所有派生的单件对象都需要挂到GameObject上
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonBehaviour<T> : BaseBehaviour where T : BaseBehaviour
{
    private static T mSingleton;

    /// <summary>
    /// 单件接口
    /// </summary>
    /// <returns>单件对象</returns>
    public static T Singleton
    {
        get
        {
            if (mSingleton == null)
            {
                mSingleton = FindObjectOfType(typeof(T)) as T;
            }
            return mSingleton;
        }
    }

//     protected override void OnDestroy()
//     {
//         mSingleton = null;
//     }
}
