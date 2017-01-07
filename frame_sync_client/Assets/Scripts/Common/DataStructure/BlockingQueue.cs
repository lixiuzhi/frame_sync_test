using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class BlockingQueue<TVal> where TVal : class
{
    private object mSyncObj;
    private List<TVal> mQueue;
         
    public BlockingQueue()
    {
        mSyncObj = new object();
        mQueue = new List<TVal>();
    }

    public void Add(TVal val)
    {
        lock(mSyncObj)
        {
            mQueue.Add(val);
        }
    }

    public TVal Get()
    {
        lock(mSyncObj)
        {
            TVal ret = null;
            if (mQueue.Count > 0)
            {
                ret = mQueue[0];
                mQueue.RemoveAt(0);
            }
            return ret;
        }
    }

    public void Clear()
    {
        lock(mSyncObj)
        {
            mQueue.Clear();
        }
    }

    public void RunAction(int len, Action<TVal> action)
    {
        lock(mSyncObj)
        {
            int cur = Math.Min(len, mQueue.Count);
            for (int a = 0; a < cur; ++a)
            {
                TVal val = Get();
                if (val != null)
                {
                    action(val);
                }
            }
        }
    }

    public bool Empty()
    {
        lock(mSyncObj)
        {
            return mQueue.Count <= 0;
        }
    }

    public int Size()
    {
        lock(mSyncObj)
        {
            return mQueue.Count;
        }
    }
}

