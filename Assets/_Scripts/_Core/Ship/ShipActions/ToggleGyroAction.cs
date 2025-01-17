using StarWriter.Core.IO;

public class ToggleGyroAction : ShipActionAbstractBase
{
    InputController inputController;

    void Start()
    {
        inputController = ship.InputController;
    }

    public override void StartAction()
    {
        inputController.OnToggleGyro(true);
    }

    public override void StopAction()
    {
        inputController.OnToggleGyro(false);
    }
}