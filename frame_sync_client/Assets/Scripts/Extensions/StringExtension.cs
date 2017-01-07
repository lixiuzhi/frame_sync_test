using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class StringExtension
{

    public static string[] Split(this string src, char sign = '+')
    {
        if (string.IsNullOrEmpty(src))
        {
            //Logger.dbg("进行split操作的字符串为空串");
            return null;
        }
        return src.Split(sign);
    }

    public static int[] SplitToIntArray(this string src, char sign = '+')
    {
        if (String.IsNullOrEmpty(src))
        {
            return new int[3] { 0, 0, 0 };
        }
        else
        {
            string[] strs = src.Split(sign);
            int[] ret = new int[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                int.TryParse(strs[i], out ret[i]);
            }
            return ret;
        }
    }

    public static List<int> SplitStringToIntList(this string src, char sign = '+')
    {
        if (String.IsNullOrEmpty(src))
        {
            return null;
        }
        else
        {
            string[] strs = src.Split(sign);
            List<int> ret = new List<int>();
            for (int i = 0; i < strs.Length; i++)
            {
                ret.Add(int.Parse(strs[i]));
            }
            return ret;
        }
    }

    public static bool IsNullOrEmpty(this string src)
    {
        return string.IsNullOrEmpty(src);
    }

    public static string ToColumn(this string src)
    {
        var ret = "";
        foreach (var tmp in src)
        {
            ret += tmp;
            ret += "\n";
        }
        ret.TrimEnd("\n".ToCharArray());
        return ret;
    }
}
