using Content.Shared.PlantAnalyzer;
using Robust.Client.UserInterface;

namespace Content.Client.Botany.Ui;

public sealed class PlantAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private PlantAnalyzerWindow? _window;
    private PlantAnalyzerScannedMessage? _pendingMessage;

    protected override void Open()
    {
        base.Open();
        _window = new PlantAnalyzerWindow();
        _window.OnClose += Close;
        _window.OpenCentered();

        if (_pendingMessage != null)
        {
            _window.DisplayInfo(_pendingMessage);
            _pendingMessage = null;
        }
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (message is not PlantAnalyzerScannedMessage cast)
            return;

        if (_window != null)
        {
            _window.DisplayInfo(cast);
        }
        else
        {
            _pendingMessage = cast;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
