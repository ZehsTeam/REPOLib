using System;

namespace REPOLib.Commands;

// TODO: Document this.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CommandExecutionAttribute : Attribute
{
    public bool RequiresDeveloperMode { get; private set; }
    public bool EnabledByDefault { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    public CommandExecutionAttribute(string? name = null, string? description = null, bool enabledByDefault = true, bool requiresDeveloperMode = false)
    {
        RequiresDeveloperMode = requiresDeveloperMode;
        EnabledByDefault = enabledByDefault;
        Name = name ?? "";
        Description = description ?? "";
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CommandInitializerAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class  CommandAliasAttribute : Attribute
{
    public string Alias { get; private set; }

    public CommandAliasAttribute(string alias)
    {
        Alias = alias;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
