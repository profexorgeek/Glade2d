using System.Linq;
using System.Reflection;
using Meadow.Hardware;

namespace Glade2d.Input;

/// <summary>
/// Represents a set of inputs that a game reacts to
/// </summary>
public abstract class GameInputSetBase
{
    internal void SetupInput(InputManager inputManager)
    {
        // Find all digital ports defined
        var digitalPortProperties = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => typeof(IDigitalInterruptPort).IsAssignableFrom(x.PropertyType))
            .ToArray();

        foreach (var digitalPortProperty in digitalPortProperties)
        {
            var port = (IDigitalInterruptPort)digitalPortProperty.GetValue(this);
            var name = digitalPortProperty.Name;
            
            inputManager.RegisterInputPort(port, name);
        }
    }
}