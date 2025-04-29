using System;

namespace REPOLib.Commands;

/// <summary>
///     // TODO: Document this.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public abstract class CommandInitializerAttribute : Attribute;