using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading; 

public class Server
{
    /// <summary>  
    /// 保存监听  
    /// </summary>  
    private Socket listener;
    /// <summary>  
    /// 端口号  
    /// </summary>  
	public int TcpPort = 8123;
    /// <summary>  
    /// IP地址  
    /// </summary>  
	public string TcpIP = "127.0.0.1"; 

    /// <summary>  
    /// 默认地址池  
    /// </summary>  
    private int Bocklog = 10; 

    /// <summary>  
    /// 关闭标志  
    /// </summary>  
    private bool ServerRunTCP = false;

    /// <summary>  
    /// 服务器运行状态  
    /// </summary>  
    public bool ServerRun
    {
        get { return ServerRunTCP; }
    }

    public int MustClientNum = 2;

    public long SyncStartTime = 0;
    
    private static int RECEIVE_MAX_SIZE = 1024 * 30;

    //
    public long frameDelta = 50;
    public long frameID=0;
    public bool isSyncStart = false;

    class ClientSockInfo
    {
        public object lockObj = new object();
        public Socket sock;
        public MyByteBuffer mByteBuffer;
        public LinkedList<CMD> cmdList = new LinkedList<CMD>();
        public int id;
    }

    Dictionary<Socket, ClientSockInfo> clients = new Dictionary<Socket, ClientSockInfo>();  
  
    public void Awake()
    {
		Logger.err("start server。。。");
        if (ServerRun == true)
        {
            Logger.err("Server Runling....");
            return;
        }

        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        listener.Bind(new IPEndPoint(IPAddress.Parse(TcpIP), TcpPort));

        listener.Listen(Bocklog);

        listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener); 
        ServerRunTCP = true;

		Timer maintime = new Timer(Update, "update",1, 50); 
    }

    public void Close()
    {
        if (ServerRun == false)
        {
            Logger.err("Server Not Run");
            return;
        }
        ServerRunTCP = false;
        listener.Close();
    }


    void Sync(object obj)
    {  
		//搜集 all 
		cmds.Clear ();
		foreach (var v in clients) {
			lock (v.Value.lockObj) {
				// Debug.LogError( v.Value.cmdList.Count);
				foreach (var v1 in v.Value.cmdList) {
					//if (v1.framID >= frameID)
						cmds.Add (v1);
				}
				v.Value.cmdList.Clear ();
			}
		}
		frameID++;
		//广播all
		foreach (var v in clients) {
			for (int i = 0; i < cmds.Count; i++) {
				cmds [i].framID = frameID;
				Send (v.Key, FrameMsgID.FrameCMD, cmds [i].Serialize ());
			}

			byte[] frameData = new byte[16];
			Array.Copy (BitConverter.GetBytes (frameID), 0, frameData, 0, 8);
			Array.Copy (BitConverter.GetBytes ((System.DateTime.Now.Ticks / 10000) - SyncStartTime - frameDelta * frameID), 0, frameData, 8, 8); 
			Send (v.Key, FrameMsgID.UpdateFrameID, frameData);
		} 
	}

    public int Count;
	List<CMD> cmds = new List<CMD>();
    void Update(object obj)
    {
        Count = clients.Count;
        if (MustClientNum == Count && !isSyncStart)
        {
            isSyncStart = true;
            //StartCoroutine(Sync());
			Timer tmr = new Timer(Sync, "sync",10, frameDelta);
			SyncStartTime = (System.DateTime.Now.Ticks +10 )/ 10000;
			foreach (var v in clients)
			{
				Send(v.Key, FrameMsgID.StartTime, BitConverter.GetBytes(SyncStartTime));
			}
			 
        }
    }

     
    int i = 0;
    private void OnConnectRequest(IAsyncResult ar)
    {
        if (ServerRunTCP == false) return;
        Socket listener = (Socket)ar.AsyncState;
        Socket sock = listener.EndAccept(ar);

        Logger.err("客户端接入");

        clients.Add(sock, new ClientSockInfo { mByteBuffer = new MyByteBuffer(RECEIVE_MAX_SIZE, false), sock = sock, id = ++i });

        if (MustClientNum > clients.Count)
        {
            listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
        }

        Logger.err(sock.RemoteEndPoint.ToString() + " Join");

        byte[] temp = new byte[128];

        sock.BeginReceive(temp, 0, 0, SocketFlags.None, new AsyncCallback(OnRecievedData), sock);


        Send(sock, FrameMsgID.ActorID, BitConverter.GetBytes(i)); 

        if (MustClientNum == clients.Count)
        {
            //发送所有actor列表给所有玩家
            foreach (var v in clients)
            {
                foreach (var v1 in clients)
                {
                    Send(v.Key, FrameMsgID.NewActor, BitConverter.GetBytes(v1.Value.id)); 
                }
            } 
        }
    }

    void disconnet(Socket sock)
    {
        clients.Remove(sock);
    }

    private void OnRecievedData(IAsyncResult ar)
    {

        Socket sock = (Socket)ar.AsyncState;

        if (ServerRunTCP == false)
        {
            sock.Close();
            return;
        }

        int RecievedSize = sock.Available;


        if (RecievedSize == 0)
        {
            sock.Close();

            return;
        }

        byte[] RecievedData = new byte[RecievedSize];

        sock.Receive(RecievedData); 

        onReceive(sock,RecievedData, RecievedSize);

        RecievedData = new byte[1024];

        try
        {
            sock.BeginReceive(RecievedData, 0, 0, SocketFlags.None, new AsyncCallback(OnRecievedData), sock);
        }
        catch (Exception ex)
        {
            disconnet(sock);
        }

    }


    private void onReceive(Socket sock,byte[] bytes, int len)
    {
        try
        {
            var clientInfo = clients[sock];
            if (len > 0 && bytes != null)
            {
                if (len > clientInfo.mByteBuffer.canUse())
                {
                    MyByteBuffer byteBuffer = new MyByteBuffer(clientInfo.mByteBuffer.maxSize() * 2 + len, false);
                    byteBuffer.copyFrom(clientInfo.mByteBuffer);
                    clientInfo.mByteBuffer = byteBuffer;
                }

                clientInfo.mByteBuffer.put(bytes, len);

                do
                {
                    if (clientInfo.mByteBuffer.remaining() < 4)
                    {
                        break;
                    }

                    int bufLen = clientInfo.mByteBuffer.readInt32(); 
                    if (bufLen > clientInfo.mByteBuffer.maxSize() || bufLen <= 0)
                    {
                        Logger.err("数据长度超过限制： " + bufLen);
                        break;
                    }

                    if (clientInfo.mByteBuffer.remaining() < bufLen)
                    {
                        clientInfo.mByteBuffer.resetHead(4);
                        //Log(EventId.Net_Error, "remaining 长度不够：" + bufLen);
                        break;
                    }

                    int msgID = clientInfo.mByteBuffer.readInt32(); 
                    //
                    byte[] content = null;
                    if (bufLen - 4 > 0)
                    {
                        content = new byte[bufLen - 4];
                        clientInfo.mByteBuffer.readBytes(content);
                    }

                    if (msgID == FrameMsgID.FrameCMD)
                    {
                        var cmd = new MoveCMD();
                        cmd.Deserialize(content);
                        lock (clientInfo.lockObj)
                        {
                            clientInfo.cmdList.AddLast(cmd);
                        }
                    }

                   // Logger.err("收到客户端数据：" + BitConverter.ToInt32(content,0)); 

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


    public void Send(Socket sock,int msgID ,byte[] data)
    {  
        var sendData = new byte[8+ data.Length];

        Array.Copy(BitConverter.GetBytes(data.Length+4), 0, sendData,0,4);
        Array.Copy(BitConverter.GetBytes(msgID), 0, sendData, 4, 4);
        Array.Copy(data, 0, sendData,8, data.Length);  

        try
        {
            sock.BeginSend(sendData,0, sendData.Length, SocketFlags.None,null,null);
        }
        catch (Exception ex)
        {
            disconnet(sock);
        } 
    }


    void OnDestroy()
    {
        foreach (var v in clients)
        {
            v.Key.Close();
        }
        listener.Close();
    }

	static void Main(string[] args)  
	{
		Server server = new Server();
		server.Awake ();

		while (true) {
			Thread.Sleep(10000);
		}

	}  
} 