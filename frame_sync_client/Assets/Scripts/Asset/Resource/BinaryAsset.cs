using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 二进制和文本资源基类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>
public class BinaryAsset :Asset
{
    public BinaryAsset(AssetManager rmgr,string name):base(rmgr,name)
    {
    }

    public override void Load()
    {
    }
    public override void Unload()
    { 
    }
}
