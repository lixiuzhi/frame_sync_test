using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public class MyByteBuffer
{
    private bool mNetOrder;
    private int mMaxSize;
    private CircularBuffer<Byte> mBuffer;

    public MyByteBuffer(int maxSize, bool netOrder)
    {
        mMaxSize = maxSize;
        mBuffer = new CircularBuffer<byte>(maxSize);
        mNetOrder = netOrder;
    }

    public void clear()
    {
        mBuffer.Clear();
    }


    public int put(byte[] bytes, int len)
    {
        return mBuffer.Put(bytes, 0, len);
    }

    public int put(byte[] bytes)
    {
        return mBuffer.Put(bytes);
    }

    public void resetHead(int size)
    {
        mBuffer.ResetHead(size);
    }

    public long readInt64()
    {
        byte[] data = new byte[8];
        mBuffer.Get(data);
        long ret = BitConverter.ToInt64(data, 0);
         return ret;

    }

    public int readInt32()
    {
        byte[] data = new byte[4];
        mBuffer.Get(data);
        int ret = BitConverter.ToInt32(data, 0);
          return ret;
        
    }
    
    public int readBytes(byte[] dst)
    {
        if (mBuffer.Size >= dst.Length)
        {
            mBuffer.Get(dst);
            return dst.Length;
        }
        else
        {
            Logger.err("剩余数据不够！" );
            return 0;
        }
    }

    public int remaining()
    {
        return mBuffer.Size;
    }

    public int maxSize()
    {
        return mMaxSize;
    }

    public void copyFrom(MyByteBuffer other)
    {
        if (other.mBuffer.Size <= 0)
            return;

        byte[] bytes = new byte[other.mBuffer.Size];
        other.readBytes(bytes);
        mBuffer.Put(bytes);
    }

    public int canUse()
    {
        return mBuffer.Capacity - mBuffer.Size;
    }
}

