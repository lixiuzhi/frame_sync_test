
/*
 * file BlockingSequenceQueue.cs
 *
 * author: 
 * date:   2014/11/6 
 */


using System;
using System.Collections.Generic;

/// <summary>
/// 线程安全的阻塞队列
/// </summary>
/// <typeparam name="T"></typeparam>
public class BlockingSequenceQueue<TKey, TVal> where TKey : class, IComparable
{
    private object mLockObject;
    private List<TKey> mSequence;
    private Map<TKey, TVal> mQueue;
    public BlockingSequenceQueue()
    {
        mLockObject = new object();
        mSequence = new List<TKey>();
        mQueue = new Map<TKey, TVal>();
    }

    /// <summary>
    /// 添加一个元素
    /// </summary>
    /// <param name="obj"></param>
    public void add(TKey key, TVal val)
    {
        lock (mLockObject)
        {
            mSequence.Add(key);
            mQueue.add(key, val);
        }
    }

    /// <summary>
    /// 根据Key返回值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TVal peek(TKey key)
    {
        lock (mLockObject)
        {
            return mQueue.get(key);
        }
    }


    /// <summary>
    /// 获得下一个元素
    /// </summary>
    /// <returns></returns>
    public TVal get()
    {
        lock (mLockObject)
        {
            TVal ret = default(TVal);
            if (mSequence.Count > 0)
            {
                TKey key = mSequence[0];
                ret = mQueue.get(key);

                //if (removed)
                //{
                    mSequence.RemoveAt(0);
                    mQueue.remove(key);
                //}
            }
            return ret;
        }
    }
    
    /// <summary>
    /// 队列是否为空
    /// </summary>
    /// <returns></returns>
    public bool empty()
    {
        lock (mLockObject)
        {
            return mSequence.Count <= 0;
        }
    }

    /// <summary>
    /// 清空队列
    /// </summary>
    public void clear()
    {
        lock (mLockObject)
        {
            mSequence.Clear();
            mQueue.clear();
        }
    }

    /// <summary>
    /// 对len长度的对象，调用action
    /// </summary>
    /// <param name="len"></param>
    /// <param name="action"></param>
    public void runAction(int len, Action<TVal> action)
    {
        lock(mLockObject)
        {
            int cur = Math.Min(len, mSequence.Count);
            for (int a = 0; a < cur; ++a)
            {
                TVal val = get();
                if (val != null)
                {
                    action(val);
                }
            }
        }
    }

    /// <summary>
    /// 删除元素
    /// </summary>
    /// <param name="key"></param>
    public void remove(TKey key)
    {
        lock (mLockObject)
        {
            for (int a = 0; a < mSequence.Count; ++a)
            {
                if (mSequence[a].CompareTo(key) == 0)
                {
                    mSequence.RemoveAt(a);
                    mQueue.remove(key);
                    break;
                }
            }
        }
    }
}

