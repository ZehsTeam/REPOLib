using BepInEx.Bootstrap;
using System.Linq;
using System.Reflection;

namespace REPOLib.Objects;

internal class AssemblyContentSource : IContentSource
{
    public string Name { get; }
    public string Version { get; }
    public string Guid { get; }
    
    public AssemblyContentSource(Assembly assembly)
    {

        var pluginInfo = Chainloader.PluginInfos.FirstOrDefault(kvp => kvp.Value.Location == assembly.Location).Value;
            
        if (pluginInfo != null)
        {
            Name = pluginInfo.Metadata.Name;
            Version = pluginInfo.Metadata.Version.ToString();
            Guid = pluginInfo.Metadata.GUID;
            
            Logger.LogDebug($"Got BepInEx plugin info for plugin {Guid} from assembly {assembly.FullName}.");
        }
        else
        {
            var assemblyName = assembly.GetName();
            
            Name = assemblyName.Name;
            Version = assemblyName.Version.ToString();
            Guid = assemblyName.FullName;
            
            Logger.LogWarning($"Failed to get BepInEx plugin info from assembly {assemblyName.FullName}, using metadata instead.");
        }
    }
}
