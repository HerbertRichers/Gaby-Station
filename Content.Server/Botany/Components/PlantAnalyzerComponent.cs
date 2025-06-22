using Robust.Shared.Audio;

namespace Content.Server.Botany.Components;

[RegisterComponent]
public sealed partial class PlantAnalyzerComponent : Component
{
    [DataField("scanDelay")]
    public float ScanDelay = 1f;

    [DataField("scanSound")] 
    public SoundSpecifier? ScanSound = new SoundPathSpecifier("/Audio/Items/Medical/healthscanner.ogg");
}