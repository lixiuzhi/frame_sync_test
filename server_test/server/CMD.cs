using System; 

public class CMD
{
    public long framID;
    public virtual void Exec()
    {

    }

    public virtual byte[] Serialize()
    {
        return null;
    }

    public virtual void Deserialize(byte[] data)
    {
        MyByteBuffer bf = new MyByteBuffer(data.Length, false);
        bf.put(data);
    } 
}

public class MoveCMD:CMD
{
    public int actorServerID = -1;
    public Int3 dir;
    public int speed = 1;
    public override void Exec()
    { 
       
    }

    public override byte[] Serialize()
    {
        byte[] data = new byte[4*7];
        Array.Copy(BitConverter.GetBytes(framID), 0, data, 0, 8);
        Array.Copy(BitConverter.GetBytes(actorServerID), 0, data, 8, 4); 
        Array.Copy(BitConverter.GetBytes(dir.x), 0, data, 12, 4); 
        Array.Copy(BitConverter.GetBytes(dir.y), 0, data, 16, 4);
        Array.Copy(BitConverter.GetBytes(dir.z), 0, data, 20, 4);
        Array.Copy(BitConverter.GetBytes(speed), 0, data, 24, 4);
        return data;
    }

    public override void Deserialize(byte[] data)
    {
        MyByteBuffer bf = new MyByteBuffer(data.Length, false);
        bf.put(data);
        framID = bf.readInt64();
        actorServerID = bf.readInt32();
        dir = new Int3(bf.readInt32(), bf.readInt32(), bf.readInt32());
        speed = bf.readInt32();
    }
}