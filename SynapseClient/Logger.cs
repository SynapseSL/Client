using BepInEx.Logging;
using UnityEngine;
using SynapseClient;
using Console = GameCore.Console;

public class Logger
{
    private static ManualLogSource BeepInExLogger => ClientBepInExPlugin.Get.Log;

    public static void Info(object s)
    {
        BeepInExLogger.LogInfo(s);
    }

    public static void Error(object s)
    {
        BeepInExLogger.LogError(s);
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