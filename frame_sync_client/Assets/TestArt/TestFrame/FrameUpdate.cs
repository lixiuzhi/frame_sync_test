using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FrameUpdate : SingletonMonoBehaviour<FrameUpdate> {

    // public bool hasSynsData = false;
    public long ClientCurFrame = 0;
    public long ServerCurFrame = 0;
    public long startFrameTime = 0;

    public uint FrameDelta = 100u;
    public int FrameSplitDelta = 25;
    public int FrameSplitCount = 4;
    public int CurFrameSplitIndex = 4;

    public long ServerDelta = 0;
    public long LocalDelta = 0;

    public Queue<CMD> recvCmdQueue = new Queue<CMD>();

    public bool isStart = false;

    // public Queue<CMD> oprCMDQueue = new Queue<CMD>();

    public MoveCMD moveCMD = new MoveCMD();

    // public int MaxOprCache = 2; 

    protected override void Awake()
    {
        base.Awake();
        NetMsgHandler.Singleton.AddListener(FrameMsgID.StartTime, SetFrameStart);
        NetMsgHandler.Singleton.AddListener(FrameMsgID.UpdateFrameID, SyncUpdateFrameID);
        NetMsgHandler.Singleton.AddListener(FrameMsgID.FrameCMD, SyncCMD);
        NetMsgHandler.Singleton.AddListener(FrameMsgID.ActorID, SetActorID);
        NetMsgHandler.Singleton.AddListener(FrameMsgID.NewActor, NewActor);
    }

    public void UpdateOprCMD()
    {
        if (!isStart)
            return;

        // var cmd = new MoveCMD { actorServerID = ActorManager.Singleton.GetPlayer().ServerId, dir = dir, speed = 2 };
        moveCMD.actorServerID = ActorManager.Singleton.GetPlayer().ServerId;
        moveCMD.dir = InputManager.Singleton.moveDir;
        moveCMD.speed = 6;
        moveCMD.framID = ServerCurFrame + 1;

        //  Debug.LogError(moveCMD.dir.ToString());

        //oprCMDQueue.Enqueue(cmd);

        //if (oprCMDQueue.Count > 1)
        //{
        //    oprCMDQueue.Dequeue();
        //}
    }

    void Update() {

        //  ASTest.Singleton.UpdateLogic((int)FrameDelta);
        InputManager.Singleton.Update();
        UpdateAllFrame();
    }

    public void NewActor(MessageData msg)
    {
        var id = BitConverter.ToInt32(msg.data, 0);

        Debug.LogError("收到new server actor id:" + id);

        if (id == ActorManager.Singleton.GetPlayer().ServerId)
            return;
        var actor = ActorManager.Singleton.CreateActor(0, ActorType.OtherPlayer, 1, 10000 + id);
        actor.ServerId = id;
    }

    public void SetActorID(MessageData msg)
    {
        ActorManager.Singleton.GetPlayer().ServerId = BitConverter.ToInt32(msg.data, 0);
        Debug.LogError("角色:" + ActorManager.Singleton.GetPlayer().ServerId);
    }

    public void SetFrameStart(MessageData msg)
    {
        startFrameTime = BitConverter.ToInt64(msg.data, 0);
        LocalDelta = System.DateTime.Now.Ticks / 10000 - startFrameTime;
        isStart = true;
        Debug.LogError("开始时间:" + startFrameTime);
    }

    public void SyncUpdateFrameID(MessageData msg)
    {
        ServerCurFrame = BitConverter.ToInt64(msg.data, 0);
        ServerDelta =  BitConverter.ToInt64(msg.data, 8); 
    }

    public void SyncCMD(MessageData msg)
    {
        MoveCMD mcmd = new MoveCMD();
        mcmd.Deserialize(msg.data);
        recvCmdQueue.Enqueue(mcmd);
        //   Debug.LogError(mcmd.framID);
    }


    void FlushUpdate()
    {
        for (; CurFrameSplitIndex < FrameSplitCount; CurFrameSplitIndex++)
        {
            ASTest.Singleton.UpdateLogic(FrameSplitDelta);
        }
        tempSplitFrameStartTime = GetLocalTime();
    }

    void UpdateSplitFrame()
    {
        if (CurFrameSplitIndex < FrameSplitCount)
        {
            long i1 = (long)((GetLocalTime() - this.tempSplitFrameStartTime)) - FrameSplitDelta;

            //    Debug.LogError(i1);
            for (int i = 0; i1 >= 0 && CurFrameSplitIndex < FrameSplitCount; i++)
            {
                CurFrameSplitIndex++;
                ASTest.Singleton.UpdateLogic(FrameSplitDelta);
            }

            if (i1 >= 0)
            {
                tempSplitFrameStartTime = GetLocalTime();
            }
        }
    }
     
    long tempSplitFrameStartTime = 0;
    void UpdateAllFrame()
    {
        UpdateSplitFrame();

        int num = (int)(this.ServerCurFrame - this.ClientCurFrame);

        if (num == 0)
        {
            UpdateOprCMD();
        }

        long num2 = (long)((GetLocalTime() - this.startFrameTime));
        while (num > 0)
        {
            long num3 = (long)(this.ClientCurFrame * this.FrameDelta);
            long nMultiFrameDelta = num2 - num3 - ServerDelta;
            if (nMultiFrameDelta >= (long)((ulong)this.FrameDelta))
            {
                FlushUpdate();

                ClientCurFrame += 1u;

                while (this.recvCmdQueue.Count > 0)
                {
                    CMD cmd = this.recvCmdQueue.Peek();
                    long num4 = (cmd.framID);
                    if (num4 > this.ClientCurFrame)
                    {
                        break;
                    }
                    //cmd.framID = num4;
                    cmd = this.recvCmdQueue.Dequeue();

                    //Debug.LogError("处理移动..");
                    cmd.Exec();
                }
                num--;

                CurFrameSplitIndex = 1;
                tempSplitFrameStartTime = GetLocalTime();
                ASTest.Singleton.UpdateLogic(FrameSplitDelta);

                //if (num > 0)
                //{
                //    for (int i = 0; i < FrameSplitCount; i++)
                //        ASTest.Singleton.UpdateLogic((int)FrameDelta / FrameSplitCount);
                //    CurFrameSplitIndex = FrameSplitCount;
                //}
                //else
                //{
                //    CurFrameSplitIndex = 1; 
                //    ASTest.Singleton.UpdateLogic((int)FrameDelta / FrameSplitCount);
                //} 
            }
            else
            {
                break;
            }
        }
        //ClientCurFrame = ServerCurFrame;
    }

    long GetLocalTime()
    {
        return System.DateTime.Now.Ticks / 10000 - LocalDelta;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 30), "同步server开始时间：" + startFrameTime);
        GUI.Label(new Rect(10, 50, 400, 30), "LocalDelta：" + LocalDelta);
        GUI.Label(new Rect(10, 90, 400, 30), "时间差：" + ((long)(GetLocalTime() - this.startFrameTime) - (long)(this.ClientCurFrame * this.FrameDelta) - ServerDelta)); 
    }
}
