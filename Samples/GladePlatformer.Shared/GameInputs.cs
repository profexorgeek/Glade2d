using Glade2d.Input;
using Meadow.Hardware;

namespace GladePlatformer.Shared;

public class GameInputs : GameInputSetBase
{
    public IDigitalInputPort? Left { get; set; }
    public IDigitalInputPort? Right { get; set; }
    public IDigitalInputPort? Jump { get; set; }
}