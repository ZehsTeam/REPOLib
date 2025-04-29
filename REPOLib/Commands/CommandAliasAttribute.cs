using System;

namespace REPOLib.Commands;

/// <summary>
///     // TODO: Document this.
/// </summary>
/// <param name="alias"></param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class CommandAliasAttribute(string alias) : Attribute
{
    /// <summary>
    ///     // TODO: Document this.
    /// </summary>
    public string Alias { get; private set; } = alias;
}