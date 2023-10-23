using Glade2d.Input;
using Meadow.Hardware;

namespace GladeInvade.Shared;

public class GameInputs : GameInputSetBase
{
    public IDigitalInputPort LeftButton { get; set; } = null!;
    public IDigitalInputPort RightButton { get; set; } = null!;
    public IDigitalInputPort ActionButton { get; set; } = null!;
}