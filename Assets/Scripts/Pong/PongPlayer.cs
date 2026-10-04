using UnityEngine;
using UnityEngine.InputSystem;

public class PongPlayer : PongPaddle
{
    private InputActions _inputs;

    private void Awake(){
        _inputs = new InputActions();
    }

    private void OnEnable()
    {
        // Enable input actions
        _inputs.Pong.Enable();

        // Subscribe to the input actions
        _inputs.Pong.Move.performed += OnMoveInputPerformed;
        _inputs.Pong.Move.canceled += OnMoveInputCancelled;
    }

    // For paddle classes, update is used to set the target position. The parent class will move
    // the paddle to reach this position in it's FixedUpdate function.
    private void Update(){
        SettingsRefresh();
    }

    private void OnDisable()
    {
        // Disable input actions
        _inputs.Pong.Disable();
    }

    // Method for handling movement input from absolute sources e.g. we should always aim to match
    // the current position of the gamepad joystick.
    private void OnMoveInputPerformed(InputAction.CallbackContext context)
    {
        var yTarget = InputUtils.GetTargetPositionFromInputValue(
            context.ReadValue<float>(),
            context.control.device,
            _paddle.transform.position.y,
            _settings.yMinimum,
            _settings.yMaximum,
            _settings.paddleSpeed
        );

        // Set target position
        _targetPosition = new Vector3(
            _paddle.transform.position.x,
            yTarget,
            _paddle.transform.position.z
        );
    }
    // When input stops always maintain current position
    private void OnMoveInputCancelled(InputAction.CallbackContext context)
    {
        _targetPosition = new Vector3(
            _paddle.transform.position.x,
            _paddle.transform.position.y,
            _paddle.transform.position.z
        );
    }
}
