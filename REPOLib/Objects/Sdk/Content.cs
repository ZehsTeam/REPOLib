using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// A base class for REPOLib Content.
/// </summary>
[PublicAPI]
public abstract class Content : ScriptableObject
{
    /// <summary>
    /// The name of this content.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Initializes this content.
    /// </summary>
    /// <param name="mod">The <see cref="Mod"/> this content belongs to.</param>
    public abstract void Initialize(Mod mod);
}
