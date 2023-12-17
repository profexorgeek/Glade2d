using Glade2d.Input;
using Meadow.Hardware;

namespace GladeInvade.Shared;

public class GameInputs : GameInputSetBase
{
    public IDigitalInterruptPort LeftButton { get; set; } = null!;
    public IDigitalInterruptPort RightButton { get; set; } = null!;
    public IDigitalInterruptPort ActionButton { get; set; } = null!;
}