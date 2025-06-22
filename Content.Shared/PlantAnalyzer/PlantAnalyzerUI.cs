using System;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

namespace Content.Shared.PlantAnalyzer;

[Serializable, NetSerializable]
public enum PlantAnalyzerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PlantAnalyzerBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly FormattedMessage Message;

    public PlantAnalyzerBoundUserInterfaceState(FormattedMessage message)
    {
        Message = message;
    }
}
