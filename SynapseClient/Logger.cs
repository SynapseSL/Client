using BepInEx.Logging;
using Il2CppSystem;

namespace SynapseClient
{
    public class Logger
    {
        internal static ManualLogSource _logger;

        public static void Info(string s)
        {
            _logger.LogInfo(s);
        }

        public static void Error(string s)
        {
            _logger.LogError(s);
        }
    }
}