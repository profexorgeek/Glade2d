using Glade2d.Input;
using Meadow.Hardware;

namespace GladeInvade.Shared;

public class GameInputs : GameInputSetBase
{
    public IDigitalInputPort LeftButton { get; set; }
    public IDigitalInputPort RightButton { get; set; }
    public IDigitalInputPort ActionButton { get; set; }
}