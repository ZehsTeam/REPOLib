namespace REPOLib.Objects;

internal enum PrefabRefResult
{
    Success,
    PrefabIdNullOrEmpty,
    PrefabNull,
    PrefabAlreadyRegistered,
    DifferentPrefabAlreadyRegistered
}

internal struct PrefabRefResponse
{
    public PrefabRefResult Result { get; set; }
    public PrefabRef? PrefabRef { get; set; }

    public PrefabRefResponse(PrefabRefResult result, PrefabRef? prefabRef)
    {
        Result = result;
        PrefabRef = prefabRef;
    }
}
