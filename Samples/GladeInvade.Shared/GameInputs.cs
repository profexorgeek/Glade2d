using Glade2d.Input;
using Meadow.Hardware;

namespace GladeInvade.Shared;

public class GameInputs : GameInputSetBase
{
    public IDigitalInputPort Left { get; set; }
    public IDigitalInputPort Right { get; set; }
    public IDigitalInputPort Action { get; set; }
}