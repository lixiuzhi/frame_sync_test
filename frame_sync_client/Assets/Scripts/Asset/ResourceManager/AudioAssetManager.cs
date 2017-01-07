using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

public class AudioAssetManager:AssetManager
{
    static AudioAssetManager ins = null;
    public static AudioAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new AudioAssetManager();
                ins.init(); 
                AssetManager.AddResourceMgr(ResourceType.Audio, ins);
            }
            return ins;
        }
    }
    protected override Asset getOrCreateResourceRefObj(string name)
    {
        var lname = name.ToLower();
        Asset res = null;
        if (!assets.TryGetValue(lname, out res))
        {
            res = new AudioAsset(this, lname);
            assets[lname] = res;
        }
        return res;
    }

    #region 
    // 音乐开关
    public bool MusicFlag
    {
        get
        {
            int value = PlayerPrefs.GetInt("MusicFlag", 0);
            if (value == 0) return true;
            else return false;
        }
        set
        {
            PlayerPrefs.SetInt("MusicFlag", value ? 0 : 1);
            if (!value)
            {
                StopBGM();
                Logger.log("关闭背景音乐");
            }
            else
            {
                mBgAudioSource.Play();
                Logger.log("打开背景音乐");
            }
        }
    }

    // 音效开关
    public bool SoundFlag
    {
        get
        {
            int value = PlayerPrefs.GetInt("SoundFlag", 0);
            if (value == 0) return true;
            else return false;
        }
        set
        {
            PlayerPrefs.SetInt("SoundFlag", value ? 0 : 1);
            if (!value)
            { 
                Logger.log("关闭音效");
            }
            else
            {
                Logger.log("打开音效"); 
            }
        }
    }

    private AudioAsset mBgAudioAsset;
    /// <summary>
    /// 背景音源
    /// </summary>
    private AudioSource mBgAudioSource; 
    /// <summary>
    /// 当前用到的音效
    /// </summary> 
    private Stack<GameObject> mPool;
    /// <summary>
    /// 语音聊天音源
    /// </summary>
    private AudioSource mChatAudioSource;   
    /// <summary>
    /// 当前播放列表
    /// </summary> 
    class AudioGObjAssetPair
    {
        public GameObject audioObj;
        public AudioAsset asset;
    }
    private Dictionary<long, AudioGObjAssetPair> mPlayingIds;
    /// <summary>
    /// 对象次最大缓存数量
    /// </summary>
    private int mMaxGameObjectPoolCount = 12; 
    /// <summary>
    /// 声音自动变小时候的音量
    /// </summary>
    private const float mVolumeSmall = 0.2f;
    /// <summary>
    /// 音源播放对象的parent对象
    /// </summary>
    private Transform audioParentTransform = null;
    private void init()
    { 
        mBgAudioSource.loop = true; 
        mPool = new Stack<GameObject>();
        mPlayingIds = new Dictionary<long, AudioGObjAssetPair>();
    }

    protected void Destroy()
    { 
        while (mPool.Count > 0)
        {
            var obj = mPool.Pop();
            if (obj != null)
                GameObject.Destroy(obj);
        } 
        mPlayingIds = null;
    }
  
    private GameObject getFromPool()
    {
        GameObject ret = null;
        if (mPool.Count != 0)
            ret = mPool.Pop();
        if (ret == null)
        {
            ret = new GameObject("audio");
            ret.transform.SetParent(audioParentTransform, false); 
            ret.getOrAddComponent<AudioSource>().playOnAwake = false;
            ret.SetActive(false);
        }
        return ret;
    }

    private void addToPool(GameObject obj)
    {
        obj.SetActive(false);
        if (mPool.Count < mMaxGameObjectPoolCount)
        {
            mPool.Push(obj);
        }
        else
        {
            GameObject.Destroy(obj);
        }
    }

    /// <summary>
    /// 移除正在播放的音效
    /// </summary>
    /// <param name="id"></param>
    public void RemoveSound(long id)
    {
        AudioGObjAssetPair pair = null;
        mPlayingIds.TryGetValue(id, out pair);
        if (pair != null)
        { 
            mPlayingIds.Remove(id);
            AudioSource audioSrc = pair.audioObj.getComponent<AudioSource>();
            pair.asset.RemoveRef();
            if (audioSrc != null)
                audioSrc.clip = null;
            addToPool(pair.audioObj);
        }
    }

   /// <summary>
   /// 播放音效
   /// </summary>
   /// <param name="name"></param>
   /// <param name="loop"></param>
   /// <returns></returns>
    public long PlaySound(string name, bool loop = false)
    {
        if (!SoundFlag)
            return -1;

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("某处的音效播放参数为空！");
            return -1;
        }

        var asset = Load(name) as AudioAsset;
        if (asset == null)
            return -1;

        GameObject audioObj = getFromPool();
        var clip = asset.GetAsset();
        var aSrc = audioObj.GetComponent<AudioSource>();
        audioObj.SetActive(true);
        aSrc.loop = loop;
        aSrc.clip = clip;
        aSrc.enabled = true;
        aSrc.Play();
        var id = IdAssginer.getId(IdAssginer.IdType.Audio);
        mPlayingIds.Add(id,new AudioGObjAssetPair { audioObj = audioObj, asset = asset });
        asset.AddRef();
        if (!loop)
        {
            CoroutineManager.Singleton.DelayedCall(clip.length, ()=>autoRemove(id));
        }
        return id;
    } 

    /// <summary>
    /// 自动移除
    /// </summary>
    /// <param name="id"></param>
    /// <param name="delayTime"></param>
    private void autoRemove(long id, float delayTime = 0.2f)
    {
        AudioGObjAssetPair pair = null;
        mPlayingIds.TryGetValue(id, out pair);
        if (pair == null)
            return;
        if (!pair.audioObj.GetComponent<AudioSource>().isPlaying)
        {
            RemoveSound(id);
        }
        else
        {
            CoroutineManager.Singleton.DelayedCall(delayTime, () => autoRemove(id));
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    private string mOldBgmName;
    public void PlayBGM(string name)
    {
        if (mBgAudioAsset != null)
            mBgAudioAsset.RemoveRef();
        mBgAudioAsset = Load(name) as AudioAsset;
        if (mBgAudioAsset == null)
            return;
        mBgAudioSource.clip = mBgAudioAsset.GetAsset();
        mBgAudioAsset.AddRef();
        mOldBgmName = name;
        if (MusicFlag && mBgAudioSource.clip != null)
            mBgAudioSource.Play();
    }

    /// <summary>
    /// 根据ID播放背景音乐
    /// </summary>
    /// <param name="id"></param>
    public void PlayBGMById(int id)
    {
        //查表 播放 playBGM 
    }
     
    public long PlaySoundById(int name, bool loop = false)
    {
        //to do..
        return 0;
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public string StopBGM()
    {
        mBgAudioSource.Stop();
        return mOldBgmName;
    } 
    #endregion
}