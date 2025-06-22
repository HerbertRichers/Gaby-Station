using Content.Shared.PlantAnalyzer;
using Robust.Client.UserInterface;

namespace Content.Client.Botany.Ui;

public sealed class PlantAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private PlantAnalyzerWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = new PlantAnalyzerWindow();
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not PlantAnalyzerBoundUserInterfaceState cast || _window == null)
            return;
        _window.DisplayMessage(cast.Message);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
