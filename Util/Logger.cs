using System;
using System.Diagnostics;

public static class Logger
{
    [Conditional("DEBUG")]
    public static void Log(string message)
    {
        UnityEngine.Debug.Log($"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff")}] {message}");
    }

    [Conditional("DEBUG")]
    public static void LogWarning(string message)
    {
        UnityEngine.Debug.LogWarning($"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff")}] {message}");
    }

    public static void LogError(string message)
    {
        UnityEngine.Debug.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff")}] {message}");
    }
}
