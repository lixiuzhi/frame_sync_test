using System.Collections;
using System.Collections.Generic;

public class CommonObjectPool <T> :SingletonTemplate<CommonObjectPool<T>> where T: IRecycleObject, new()
{
    Stack<T> pool = new Stack<T>(); 

    public static T Get()
    {
        if (Singleton.pool.Count == 0)
        {
            return new T();
        }
        var t = Singleton.pool.Pop();
        t.ReAlloc();
        return t;
    }

    public static void Recycle(T obj)
    {
        if (obj == null)
        {
            return;
        }
        obj.Recycle();
        Singleton.pool.Push(obj);
    } 
}
