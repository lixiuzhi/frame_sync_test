using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Collections.Generic;

public class Client : MonoBehaviour {
 
    /// <summary>  
    /// 端口号  
    /// </summary>  
    public int ServerPort = 8000;
    /// <summary>  
    /// IP地址  
    /// </summary>  
    public string ServerIP = "127.0.0.1";

    MyByteBuffer mByteBuffer = new MyByteBuffer(RECEIVE_MAX_SIZE, false);
         
    private static int RECEIVE_MAX_SIZE = 1024 * 30;

    Socket socket;

    Queue<MessageData> msgQueue = new Queue<MessageData>(); 

    object lockObj = new object();

    IEnumerator Start()
    { 
            yield return new WaitForSeconds(2);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  
        try
        {
            socket.Connect(ServerIP, ServerPort);
            socket.BeginReceive(new byte[128], 0, 0, SocketFlags.None, new AsyncCallback(OnRecievedData), socket);
        }
        catch (Exception ex)
        {
            Logger.err(ex.Message);
        }

        StartCoroutine_Auto(SendCMD());
    }

    int i = 0;
    void Update()
    {
        lock (lockObj)
        {
            while (msgQueue.Count > 0)
            {
                var msg = msgQueue.Dequeue();
                NetMsgHandler.Singleton.DispatchEvent(msg); 
            }
        }
    }

    

    private void OnRecievedData(IAsyncResult ar)
    {
        Socket sock = (Socket)ar.AsyncState;
        
        int RecievedSize = sock.Available;

        if (RecievedSize == 0)
        { 
            return;
        }

        byte[] RecievedData = new byte[RecievedSize];

        int len = sock.Receive(RecievedData);

     //   Logger.err(sock.RemoteEndPoint.ToString() + " Read " + RecievedSize.ToString());

        onReceive(RecievedData, len);

        RecievedData = new byte[1024];

        try
        {
            sock.BeginReceive(RecievedData, 0, 0, SocketFlags.None, new AsyncCallback(OnRecievedData), sock);
        }
        catch (Exception ex)
        {
            sock.Close();
        }
    }
    
    private void onReceive( byte[] bytes, int len)
    {
        try
        {
            if (len > 0 && bytes != null)
            {
                if (len > mByteBuffer.canUse())
                {
                    MyByteBuffer byteBuffer = new MyByteBuffer(mByteBuffer.maxSize() * 2 + len, false);
                    byteBuffer.copyFrom(mByteBuffer);
                    mByteBuffer = byteBuffer;
                }

                mByteBuffer.put(bytes, len);

                do
                {
                    if (mByteBuffer.remaining() < 4)
                    {
                        break;
                    }

                    int tmp = mByteBuffer.readInt32();
                    int bufLen = (tmp & 0x7FFFFF);
                    if (bufLen > mByteBuffer.maxSize() || bufLen <= 0)
                    {
                        Debug.LogError("数据长度超过限制： " + bufLen);
                        break;
                    }

                    if (mByteBuffer.remaining() < bufLen)
                    {
                        mByteBuffer.resetHead(4);
                        //Log(EventId.Net_Error, "remaining 长度不够：" + bufLen);
                        break;
                    }

                    int msgID = mByteBuffer.readInt32();  

                    //
                    byte[] content = null;
                    if (bufLen - 4 > 0)
                    {
                        content = new byte[bufLen - 4];
                        mByteBuffer.readBytes(content);
                    }

                    lock (lockObj)
                    {
                        msgQueue.Enqueue(new MessageData { msgID  = msgID,data = content});
                    }

                   // Logger.err("收到服务器数据：" + BitConverter.ToInt32(content, 0));

                    //////////MessageUtils.sendToGameLogic(msgId, content); 
                }
                while (true);
            }

        }
        catch (Exception ex)
        {
            Logger.err(ex.ToString());
        }
    }


    public void Send(int msgID,byte[] data)
    {
        int len = 8 + data.Length;

        var sendData = new byte[8 + data.Length];

        Array.Copy(BitConverter.GetBytes(data.Length+4), 0, sendData, 0, 4);
        Array.Copy(BitConverter.GetBytes(msgID), 0, sendData, 4, 4);
        Array.Copy(data, 0, sendData, 8, data.Length);

        try
        {
            socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, null);
        }
        catch (Exception ex)
        {
            socket.Close();
        }
    }


    IEnumerator SendCMD()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(FrameUpdate.Singleton.FrameDelta * 0.001f);
            if (FrameUpdate.Singleton.isStart)
            {
                //foreach (var v in FrameUpdate.Singleton.oprCMDQueue)
                //{
                //    Send(FrameMsgID.FrameCMD, v.Serialize());
                //  //  Debug.LogError("SEND:" + v.framID);
                //}
                //FrameUpdate.Singleton.oprCMDQueue.Clear();
                Send(FrameMsgID.FrameCMD,FrameUpdate.Singleton.moveCMD.Serialize());
            }
        }
    }

    void OnDestroy()
    {
        socket.Close();
    }
}
