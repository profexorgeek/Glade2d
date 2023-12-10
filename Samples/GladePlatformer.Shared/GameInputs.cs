using Glade2d.Input;
using Meadow.Hardware;

namespace GladePlatformer.Shared;

public class GameInputs : GameInputSetBase
{
    public IDigitalInterruptPort? Left { get; set; }
    public IDigitalInterruptPort? Right { get; set; }
    public IDigitalInterruptPort? Jump { get; set; }
}