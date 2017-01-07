using UnityEngine;

/// <summary>
/// 单件模版 
/// 所有派生的单件对象都需要挂到GameObject上
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Singleton;

    protected virtual void Awake()
    {
        Singleton = this as T;
    }

}
