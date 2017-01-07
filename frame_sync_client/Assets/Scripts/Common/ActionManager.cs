
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 动画播放管理器
/// </summary>
public class ActionManager
{
    private class KeyFrame
    {
        public string Name { get; set; } 
        public float CallbackTime { get; set; }
        public Action Callback { get; set; }  

        public bool IsCalled { set; get; }
        public KeyFrame(string name, float callbackTime, Action callback)
        {
            Name = name; 
            CallbackTime = callbackTime;
            Callback = callback;
            IsCalled = false;
        } 
    }

    private float timeLength = 0;
    private List<KeyFrame> callbacks;
    private Action playEndCallback;
    private bool playing = false;
    private float timedelta = 0;

    public ActionManager(float totalTime)
    {
        timeLength = totalTime;
        callbacks = new List<KeyFrame>(); 
    }

    public void Reset()
    {
        playing = false;
        if (callbacks != null)
            callbacks.Clear();
        playEndCallback = null;
        timedelta = 0;
    }
    
    /// <summary>
    /// 注册回调
    /// </summary> 
    public void RegisterKeyFrameCallback(string name, float callbackTime, Action callback)
    {
        callbacks.Add(new KeyFrame(name, callbackTime,callback)); 
    } 
  
    public void Play(Action playEndCallback)
    {
        playing = true;
    } 
   

    public void OnUpdate(float deltaTime)
    {
        if (playing)
        {
            timedelta += Time.deltaTime;
            for (int i = 0; i < callbacks.Count; i++)
            {
                var call = callbacks[i];
                if (!call.IsCalled && timedelta>=call.CallbackTime)
                {
                    call.Callback();
                    call.IsCalled = true;
                }
            }

            if(timedelta>=timeLength)
            {
                if (playEndCallback != null)
                    playEndCallback();
                Reset(); 
            }
        }
    }
}

