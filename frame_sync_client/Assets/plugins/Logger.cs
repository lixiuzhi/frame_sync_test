using System;
using System.IO;
using System.Threading;

public class Logger
{
    private static bool mPrintInConsole;
    private static string mLogPath;
    private static bool mWriteToFile;

    public static void Init(bool printInConsole, string logPath, bool writeToFile)
    {
        mPrintInConsole = printInConsole;
        mLogPath = logPath;
        mWriteToFile = writeToFile;
    }

    /// <summary>
    /// debug 信息
    /// </summary>
    /// <param name="msg"></param>
    public static void dbg(string msg)
    {
        string outStr = msg + "--[" + DateTime.Now.ToString() + "][DBG]";
        if (mPrintInConsole)
        { 
            string printStr = "<color=green>[" + outStr + "</color>";
            UnityEngine.Debug.Log(printStr);
        }
        write2File(outStr);
    }

    /// <summary>
    /// warn 信息
    /// </summary>
    /// <param name="msg"></param>
    public static void wrn(string msg)
    {
        string outStr = "[" + DateTime.Now.ToString() + "][WRN]--" + msg;

        if (mPrintInConsole)
        {
            string printStr = "<color=yellow>[" + outStr + "</color>";
            UnityEngine.Debug.LogWarning(printStr);
        }
        write2File(outStr);
    }

    /// <summary>
    /// error 信息
    /// </summary>
    /// <param name="msg"></param>
    public static void err(string msg)
    {
        string outStr = "[" + DateTime.Now.ToString() + "][ERR]--" + msg;
        
        if (mPrintInConsole)
        {
            string printStr = "<color=red>[" + outStr + "</color>";
            UnityEngine.Debug.LogError(printStr);
        }
        write2File(outStr);
    }

    /// <summary>
    /// 其他线程调用的日志，无输出
    /// </summary>
    /// <param name="msg"></param>
    public static void log(string msg)
    {
        string outStr = "[" + DateTime.Now.ToString() + "][LOG]--" + msg;
        if (mPrintInConsole)
        {
            string printStr = "<color=yellow>[" + outStr + "</color>";
            UnityEngine.Debug.Log(printStr);
        }
        write2File(outStr);
    }

    /// <summary>
    /// 将日志写到磁盘
    /// </summary>
    /// <param name="str"></param>
    private static void write2File(string str, int threadId = 0)
    {
        if (!mWriteToFile)
            return;

        DateTime now = DateTime.Now;
        string logFile = string.Format("{0}/Log_{1}_{2}_{3}_{4}.log", mLogPath, 
            now.Year, now.Month, now.Day, threadId);

        StreamWriter sw;
        if (!File.Exists(logFile))
            sw = File.CreateText(logFile);
        else
            sw = new StreamWriter(logFile, true);

        sw.WriteLine(str);
        sw.Dispose();
    }
}
