using BepInEx.Logging;

namespace SynapseClient.Platform;

public class Logger
{
    internal static ManualLogSource BeepInExLogger;

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
        //Console.AddLog(s.ToString(), Color.gray, false);
    }
}