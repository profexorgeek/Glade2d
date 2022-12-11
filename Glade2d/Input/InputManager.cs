using System;
using System.Collections.Generic;

namespace Glade2d.Input
{
    /// <summary>
    /// Tracks the state of different input mechanisms
    /// </summary>
    public class InputManager
    {
        private readonly Dictionary<string, ButtonState> _buttons = new();
        private readonly Queue<KeyValuePair<string, ButtonState>> _pendingButtonStates = new();
        private readonly List<string> _pressedButtons = new();

        /// <summary>
        /// Applies any pending state changes for inputs. This is done for uncertainty of if button press events can
        /// happen mid frame or not, and essentially guarantees that any button state changes after this is called
        /// do not cause inconsistent states within a single frame.
        /// </summary>
        internal void Tick()
        {
            // First reset any buttons in the pressed state, as we have advanced to the next frame and therefore
            // they are no longer pressed.
            foreach (var name in _pressedButtons)
            {
                if (_buttons[name] == ButtonState.Pressed)
                {
                    _buttons[name] = ButtonState.Up;
                }
            }
            
            _pressedButtons.Clear();
            
            // Apply pending states
            while (_pendingButtonStates.TryDequeue(out var pendingState))
            {
                if (!_buttons.TryGetValue(pendingState.Key, out var oldState))
                {
                    _buttons.Add(pendingState.Key, pendingState.Value);
                }
                
                else if (pendingState.Value == ButtonState.Up && oldState == ButtonState.Down)
                {
                    _buttons[pendingState.Key] = ButtonState.Pressed;
                    _pressedButtons.Add(pendingState.Key);
                }

                else
                {
                    _buttons[pendingState.Key] = pendingState.Value;
                }
            }
            
            _pendingButtonStates.Clear();
        }

        /// <summary>
        /// Retrieves the current state of the specified button. If we have never received a button event with that
        /// name then `ButtonState.Up` is returned.
        /// </summary>
        public ButtonState GetButtonState(string inputName)
        {
            return _buttons.TryGetValue(inputName, out var state)
                ? state
                : ButtonState.Up;
        }

        /// <summary>
        /// Notifies the input manager that a button with the specified name has been pushed down
        /// </summary>
        public void ButtonPushed(string inputName)
        {
            Console.WriteLine($"Button {inputName} pressed");
            _pendingButtonStates.Enqueue(new KeyValuePair<string, ButtonState>(inputName, ButtonState.Down));
        }

        /// <summary>
        /// Notifies that a button with the specified name is no longer being pushed down
        /// </summary>
        public void ButtonReleased(string inputName)
        {
            Console.WriteLine($"Button {inputName} released");
            _pendingButtonStates.Enqueue(new KeyValuePair<string, ButtonState>(inputName, ButtonState.Up));
        }
    }
}