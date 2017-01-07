using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
 
 
public class EditorTools
{
    public static T parseValueToEnum<T>(int val)
    {
        return (T)Enum.Parse(typeof(T), val + "", true);
    }

    public static T parseValueToEnum<T>(string val)
    {
        return (T)Enum.Parse(typeof(T), val, true);
    }

    private static bool EqualStrArray(string[] strList1, string[] strList2)
    {
        if (strList1 == null || strList2 == null)
            return false;

        if (strList1.Length != strList2.Length)
            return false;

        for (int i = 0; i < strList1.Length; ++i)
        {
            if (strList1[i] != strList2[i])
                return false;
        }

        return true;
    }

    public static void ClearFolder(string dir)
    {
        if (!Directory.Exists(dir))
            return;
        if (dir.Contains("svn"))
            return;
        DirectoryInfo d0 = new DirectoryInfo(dir);
        if (d0.Attributes.ToString().Contains("Hidden"))
            return;

        foreach (string d in Directory.GetFileSystemEntries(dir))
        {
            FileInfo fi = new FileInfo(d);
            if (File.Exists(d) && fi.Attributes.ToString().IndexOf("Hidden") == -1)
            {
                try
                { 
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                DirectoryInfo d1 = new DirectoryInfo(d);
                if (d1.GetFiles().Length != 0)
                {
                    ClearFolder(d1.FullName);
                }
                //Directory.Delete(d);
            }
        }
    }
    /// <summary>
    /// 拷贝目录
    /// </summary>
    /// <param name="fromDir"></param>
    /// <param name="toDir"></param>
    public static void CopyDir(string fromDir, string toDir)
    {
        if (!Directory.Exists(fromDir))
            return;

        if (!Directory.Exists(toDir))
        {
            Directory.CreateDirectory(toDir);
        }

        string[] files = Directory.GetFiles(fromDir);
        foreach (string formFileName in files)
        {
            if (formFileName.Contains(".svn"))
                continue;
            string fileName = Path.GetFileName(formFileName);
            string toFileName = Path.Combine(toDir, fileName);
            File.Copy(formFileName, toFileName,true);
        }
        string[] fromDirs = Directory.GetDirectories(fromDir);
        foreach (string fromDirName in fromDirs)
        {
            if (fromDirName.Contains(".svn"))
                continue;
            string dirName = Path.GetFileName(fromDirName);
            string toDirName = Path.Combine(toDir, dirName);
            CopyDir(fromDirName, toDirName);
        }
    }

    public static void SetLayer(GameObject go, int layer)
    {
        Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in transforms)
        {
            t.gameObject.layer = layer;
        }
    }
}