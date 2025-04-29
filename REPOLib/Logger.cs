using BepInEx.Logging;
using JetBrains.Annotations;

namespace REPOLib;

[PublicAPI]
internal static class Logger
{
    private static ManualLogSource? ManualLogSource { get; set; }

    public static void Initialize(ManualLogSource manualLogSource)
        => ManualLogSource = manualLogSource;

    public static void LogDebug(object data)
        => Log(LogLevel.Debug, data);

    public static void LogInfo(object data, bool extended = false)
        => Log(LogLevel.Info, data, extended);
    
    public static void LogWarning(object data, bool extended = false)
        => Log(LogLevel.Warning, data, extended);

    public static void LogError(object data, bool extended = false) 
        => Log(LogLevel.Error, data, extended);
    
    public static void LogFatal(object data, bool extended = false)
        => Log(LogLevel.Fatal, data, extended);

    public static void Log(LogLevel logLevel, object data, bool extended = false)
    {
        if (extended && !IsExtendedLoggingEnabled()) return;
        ManualLogSource?.Log(logLevel, data);
    }

    public static bool IsExtendedLoggingEnabled() 
        => ConfigManager.ExtendedLogging?.Value == true;
}
