namespace REPOLib.Objects;

public interface IContentSource
{
    string Name { get; }
    string Version { get; }
    string Guid { get; }
}