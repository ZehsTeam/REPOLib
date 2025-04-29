using System;

namespace REPOLib.Commands;

/// <summary>
///     // TODO: Document this.
/// </summary>
/// <param name="name"></param>
/// <param name="description"></param>
/// <param name="enabledByDefault"></param>
/// <param name="requiresDeveloperMode"></param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class CommandExecutionAttribute(
    string? name = null,
    string? description = null,
    bool enabledByDefault = true,
    bool requiresDeveloperMode = false)
    : Attribute
{
    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public bool RequiresDeveloperMode { get; private set; } = requiresDeveloperMode;

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public bool EnabledByDefault { get; private set; } = enabledByDefault;

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public string Name { get; private set; } = name ?? "";

    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public string Description { get; private set; } = description ?? "";
}