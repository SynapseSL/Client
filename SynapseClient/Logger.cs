using BepInEx.Logging;
using UnityEngine;
using Console = GameCore.Console;

public class Logger
{
    internal static ManualLogSource _logger;

    public static void Info(object s)
    {
        _logger.LogInfo(s);
    }

    public static void Error(object s)
    {
        _logger.LogError(s);
    }

    public static void AddGCLog(object s)
    {
        Console.AddLog(s.ToString(), Color.gray, false);
    }

    public static void AddGCLog(object s, Color color)
    {
        Console.AddLog(s.ToString(), color, false);
    }
}