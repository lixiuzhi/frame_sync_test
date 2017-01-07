
/*
 * file GameObjectExtenions.cs
 *
 * author: 
 * date:   2014/09/16 
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// GameObject扩展类
/// </summary>
public static class GameObjectExtension
{
    /// <summary>
    /// 获得一个组件，不存在则添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T getOrAddComponent<T>(this GameObject go) where T : Component
    {
        T ret = go.GetComponent<T>();
        if (ret == null)
            ret = go.AddComponent<T>();
        return ret;
    }

    public static T getComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
            Debug.LogError(go.name + "can not find component : " + typeof(T).ToString());
        return comp;
    }

    //public static Transform TransformExt(this GameObject go)
    //{
    //    return getOrAddComponent<Transform>(go);
    //}

    /// <summary>
    /// GetComponent获取接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inObj"></param>
    /// <returns></returns>
    public static T GetInterface<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");

            return null;
        }
        var tmps = inObj.GetComponents<Component>().OfType<T>();
        if (tmps.Count() == 0) return null;
        return tmps.First();
    }

    /// <summary>
    /// GetComponent获取所用接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inObj"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetInterfaces<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return Enumerable.Empty<T>();
        }
        return inObj.GetComponents<Component>().OfType<T>();
    }


    public static GameObject[] getChilds(this GameObject obj)
    {
        GameObject[] objs = new GameObject[obj.transform.childCount];
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            objs[i] = obj.transform.GetChild(i).gameObject;
        }
        return objs;
    }

    public static void removeComponent<T>(this GameObject obj) where T: Component
    {
        GameObject.Destroy(obj.GetComponent<T>());
    }


    /// <summary> 
    /// 利用反射来判断对象是否包含某个属性 
    /// </summary>
    /// <param name="instance">object</param> 
    /// <param name="propertyName">需要判断的属性</param> 
    /// <returns>是否包含</returns> 
    //public static bool ContainProperty(this object instance, string propertyName)
    //{
    //    if (instance != null && !string.IsNullOrEmpty(propertyName))
    //    {
    //        FieldInfo field = instance.GetType().GetField(propertyName);
    //        return (field != null);
    //    }
    //    return false;
    //}


    public static void setLayer(this GameObject obj, string layerName)
    {
        if (obj == null)
            return;
        int layer = LayerMask.NameToLayer(layerName);
        Transform[] childs = obj.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in childs)
            child.gameObject.layer = layer;
    }

}

