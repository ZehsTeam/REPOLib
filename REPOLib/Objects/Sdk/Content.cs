using UnityEngine;

namespace REPOLib.Objects.Sdk;

public abstract class Content : ScriptableObject
{
    public abstract string Name { get; }
    public abstract void Initialize(Mod mod);
}
