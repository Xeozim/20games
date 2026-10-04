using UnityEngine;
using UnityEngine.InputSystem;

public static class InputUtils
{
    // Calculate a target position on a 1D axis based on the input value and the device type.
    // This is useful for handling different input devices (mouse, touchscreen, joystick, gamepad) in a consistent way.
    public static float GetTargetPositionFromInputValue(float inputValue, InputDevice device, float current, float min, float max, float speed = 1)
    {
        // Target position in range 0 (target = min) to 1 (target = max)
        // Adjust mouse / touchscreen inputs to be relative to the screen
        // For gamepad / joystick inputs, make the target position relative to the current position
        // For other inputs, convert value from range -1 => 1 to 0 => 1
        var target01 = device switch
        {
            Mouse or Touchscreen => inputValue / Screen.width,
            Joystick or Gamepad => current + inputValue * Time.deltaTime * speed,
            _ => (inputValue + 1.0f) * 0.5f,
        };

        return min + (target01 * (max - min));
    }
}