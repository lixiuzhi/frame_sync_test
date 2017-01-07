

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

/// <summary>
/// 用于所有的Behavior类继承
/// </summary> 
public class BaseBehaviour : MonoBehaviour
{
    // 缓存的Component
    protected Map<string, Component> mComponents = new Map<string, Component>();
    // 缓存的GameObject
    protected GameObject mGameObject = null;
    // 启动的协程列表
    protected List<long> mCoroutines = new List<long>();

    protected ObjectGameEventSet mEventUtils = new ObjectGameEventSet();

    /// 获得一个组件，如果该组件不存在，则添加一个到对象上
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetOrAddComponent<T>() where T : Component
    {
        string key = typeof(T).Name;
        Component exists = mComponents.get(key);
        if (exists == null)
        {
            exists = GameObjectExt.getOrAddComponent<T>();
            mComponents.add(key, exists);
        }
        return (T)exists;
    }


    /// <summary>
    /// 添加组件
    /// </summary>
    /// <param name="type"></param>
    public void AddComponent(Type type)
    {
        if (GameObjectExt != null)
            GameObjectExt.AddComponent(type);
    }

    /// <summary>
    /// 缓存的Transform
    /// </summary>
    public Transform TransformExt
    {
        get { return GetOrAddComponent<Transform>(); }
    }

    /// <summary>
    /// 缓存的GameObject
    /// </summary>
    public GameObject GameObjectExt
    {
        get
        {
            if (mGameObject == null)
                mGameObject = gameObject;
            return mGameObject;
        }
    }


    public virtual void Awake()
    {
    }

    public virtual void Start()
    {
 
    }

    public virtual void Update()
    {
 
    }

    public virtual void OnDisable()
    {
        stopAllCoroutine();
    }

    protected virtual void OnDestroy()
    {
        stopAllCoroutine();
        RemoveAllListener();
    }

    protected void stopAllCoroutine()
    {
        foreach (long id in mCoroutines)
        {
            CoroutineManager.Singleton.RemoveCoroutine(id);
        }
        mCoroutines.Clear();
    }
    
    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="delayedTime"></param>
    /// <param name="callback"></param>
    /// <returns></returns> 
    public long DelayCall(float delayedTime, Action<object> callback, object param)
    {
        long ret = CoroutineManager.Singleton.DelayedCall(delayedTime, callback, param);
        mCoroutines.Add(ret);
        return ret;
    } 

    /// <summary>
    /// 取消一个延迟调用
    /// </summary>
    /// <param name="id"></param>
    public void CancelDelayCall(long id)
    {
        DeleteCoroutine(id);  
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    /// <param name="co"></param>
    /// <returns></returns>
    public long NewCoroutine(IEnumerator co)
    {
        long ret = CoroutineManager.Singleton.AddCoroutine(co);
        mCoroutines.Add(ret);
        return ret;
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="id"></param>
    public void DeleteCoroutine(long id)
    {
        CoroutineManager.Singleton.RemoveCoroutine(id);
        int idx = mCoroutines.IndexOf(id);
        if (idx >= 0)
        {
            mCoroutines.RemoveAt(idx);
        }
    }

    /// <summary>
    /// 根据路径获取子节点 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Transform GetChild(string path)
    {
        return TransformExt.Find(path);
    }
     
	/// <summary>
	/// 添加事件
	/// </summary>
	protected void AddListener( EventID evtId, EventAction callBack )
	{
        mEventUtils.AddListener(evtId, callBack);
	}
	
	/// <summary>
	/// 移除事件
	/// </summary>
	protected void RemoveListener( EventID evtId, EventAction callBack )
	{
        mEventUtils.RemoveListener(evtId, callBack);
	}
	
	protected void RemoveAllListener()
	{
        mEventUtils.RemoveAllListener();
	}
}


