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

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (_window == null)
            return;

        if (message is not PlantAnalyzerScannedMessage cast)
            return;

        _window.DisplayInfo(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
